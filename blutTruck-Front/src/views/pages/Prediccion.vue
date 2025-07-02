<script>
import axios from 'axios';
import { mapState } from 'pinia';
import { useUserStore } from '/stores/user'; // Adjust path if necessary

export default {
    data() {
        // ... (data properties remain the same) ...
        return {
            riskPercentage: 45,
            apiRiskPercentage: null,
            isLoading: false,
            apiError: null,
            recommendations: [],
            monitoredPeople: [],
            isListLoading: false,
            listApiError: null,
            isInfinite: true,
            currentTranslate: 0,
            isPredictingManually: false,
            manualPredictionError: null
        };
    },
    computed: {
        // ... (userId, displayPercentage, riskMessage remain the same) ...
        ...mapState(useUserStore, {
            userId: (store) => store.fullUserData?.userId
        }),
        displayPercentage() {
            return this.apiRiskPercentage !== null && !isNaN(this.apiRiskPercentage) ? parseFloat(this.apiRiskPercentage.toFixed(1)) : this.riskPercentage;
        },
        riskMessage() {
            const percentage = this.displayPercentage;
            if (percentage < 30) return 'Bajo riesgo';
            if (percentage < 70) return 'Riesgo moderado';
            return 'Alto riesgo';
        },
        staticStyle() {
            // Only apply translation in static mode (when NOT infinite)
            if (!this.isInfinite) {
                return { transform: `translateX(${this.currentTranslate}px)` };
            }
            // Reset transform when switching to infinite if needed, handled by CSS now
            return {};
        },
        // --- NEW Computed property for dynamic infinite scroll styles ---
        infiniteScrollStyles() {
            if (this.isInfinite && this.monitoredPeople.length > 0) {
                // Calculate width: card (160px) + margin-right (2rem ~ 32px) = 192px
                const totalWidth = this.monitoredPeople.length * 192;
                // Calculate duration: 5 seconds per person (adjust as needed)
                const duration = this.monitoredPeople.length * 5;
                return {
                    // Set CSS variables (--*) for use in the <style scoped> block
                    '--scroll-width': `-${totalWidth}px`,
                    '--scroll-duration': `${duration}s`
                };
            }
            // Return empty object or default values if not infinite scrolling
            return {
                '--scroll-width': '0px',
                '--scroll-duration': '30s' // Default duration
            };
        }
    },
    methods: {
        // ... (faceImage, toggleCarousel, prevSlide, nextSlide, fetchPredictionData, fetchMonitoredList, triggerManualPrediction remain the same) ...
        faceImage(percentage) {
            if (percentage === null || typeof percentage === 'undefined' || isNaN(percentage)) {
                return '/demo/images/caras/gris.png';
            }
            if (percentage < 30) return '/demo/images/caras/verde.png';
            if (percentage < 70) return '/demo/images/caras/amarilla.png';
            return '/demo/images/caras/roja.png';
        },
        toggleCarousel() {
            this.isInfinite = !this.isInfinite;
            this.currentTranslate = 0; // Reset position when toggling
        },
        prevSlide() {
            this.currentTranslate += 200;
            if (this.currentTranslate > 0) this.currentTranslate = 0;
        },
        nextSlide() {
            this.currentTranslate -= 200;
            // Add more robust bounds checks if needed for static mode
        },
        async fetchPredictionData() {
            if (!this.userId) {
                this.apiError = 'No se pudo obtener el ID de usuario.';
                return;
            }
            this.isLoading = true;
            this.apiError = null;
            const apiUrl = 'https://bluttruck.grial.eu/backend/api/ReadData/getprediction';
            const payload = { UserId: this.userId, IdToken: 'string' }; // IdToken debería ser real

            try {
                const response = await axios.post(apiUrl, payload);
                if (response.data && typeof response.data.resultado === 'string') {
                    const parsedPercentage = parseFloat(response.data.resultado);
                    if (!isNaN(parsedPercentage)) {
                        this.apiRiskPercentage = parsedPercentage;
                        this.recommendations = response.data.alertas || [];
                    } else {
                        throw new Error(`Valor de predicción inicial inválido: "${response.data.resultado}".`);
                    }
                } else {
                    // Esto maneja el caso de 200 OK con resultado nulo
                    this.apiRiskPercentage = null;
                    this.recommendations = response.data?.alertas || [];
                }
            } catch (error) {
                console.error('Error fetchPredictionData:', error);

                // ✅ ¡LÓGICA AÑADIDA!
                // Si el error es un 404, trátalo como "no hay datos", no como un error.
                if (error.response && error.response.status === 404) {
                    console.warn('API devolvió 404: No hay datos de predicción existentes.');
                    this.apiRiskPercentage = null; // No hay predicción
                    this.recommendations = [];
                    this.apiError = null; // Limpiamos el error porque no es un fallo técnico
                } else {
                    // Para cualquier otro error (500, error de red, etc.), sí mostramos el mensaje.
                    this.apiError = `Error (predicción inicial): ${error.response?.data?.Message || error.message || 'Desconocido'}`;
                    this.apiRiskPercentage = null;
                    this.recommendations = [];
                }
            } finally {
                this.isLoading = false;
            }
        },
        async fetchMonitoredList() {
            // (Code is identical to previous correct version)
            if (!this.userId) {
                this.listApiError = 'No se pudo obtener el ID de usuario para la lista.';
                return;
            }
            this.isListLoading = true;
            this.listApiError = null;
            this.monitoredPeople = [];
            const apiUrl = 'https://bluttruck.grial.eu/backend/api/ReadData/getlistprediction';
            const payload = { UserId: this.userId, IdToken: 'string' };
            try {
                console.log('Fetching monitored list for UserId:', this.userId);
                const response = await axios.post(apiUrl, payload);
                if (response.data && Array.isArray(response.data)) {
                    this.monitoredPeople = response.data
                        .map((item) => {
                            const percentageStr = item.resultado;
                            const name = item.nombre || `Usuario ${item.userId || 'Desconocido'}`;
                            if (typeof percentageStr === 'string') {
                                const percentage = parseFloat(percentageStr);
                                if (!isNaN(percentage)) {
                                    return { name: name, percentage: parseFloat(percentage.toFixed(1)) };
                                } else {
                                    console.warn(`Invalid percentage string "${percentageStr}" for user ${name}. Skipping.`);
                                    return null;
                                }
                            } else {
                                console.warn(`Missing or invalid 'resultado' type for user ${name}. Skipping.`);
                                return null;
                            }
                        })
                        .filter((person) => person !== null);
                    console.log('API Monitored List Received:', this.monitoredPeople);
                    if (this.monitoredPeople.length === 0 && response.data.length > 0) {
                        console.warn('Processed list is empty, check API response format.');
                        this.listApiError = 'Error procesando la lista recibida (verificar formato nombre/resultado).';
                    } else if (this.monitoredPeople.length === 0) {
                        console.log('No monitored people data returned or all entries were invalid.');
                    }
                } else {
                    throw new Error('Respuesta inválida (lista): No se recibió un array.');
                }
            } catch (error) {
                console.error('Error fetchMonitoredList:', error);
                this.listApiError = `Error (lista): ${error.response?.data?.Message || error.message || 'Desconocido'}`;
                this.monitoredPeople = [];
            } finally {
                this.isListLoading = false;
            }
        },
        async triggerManualPrediction() {
            // (Code is identical to previous correct version)
            if (!this.userId) {
                this.manualPredictionError = 'No se pudo obtener el ID de usuario para realizar la predicción.';
                return;
            }
            if (this.isPredictingManually) {
                return;
            }
            console.log('Triggering manual prediction for UserId:', this.userId);
            this.isPredictingManually = true;
            this.manualPredictionError = null;
            this.apiError = null;
            const apiUrl = 'https://bluttruck.grial.eu/backend/api/Prediccion/PredictHealthRisk';
            const payload = { UserId: this.userId };
            try {
                const response = await axios.post(apiUrl, payload);
                if (response.data) {
                    const parsedPercentage = parseFloat(response.data.prediccion);
                    if (!isNaN(parsedPercentage)) {
                        this.apiRiskPercentage = parsedPercentage;
                        console.log('Manual prediction successful:', this.apiRiskPercentage);
                        if (response.data.AlertasEspecificasGeneradas && Array.isArray(response.data.AlertasEspecificasGeneradas)) {
                            this.recommendations = response.data.AlertasEspecificasGeneradas;
                            console.log('Updated recommendations:', this.recommendations);
                        } else {
                            this.recommendations = [];
                            console.log('No specific alerts provided.');
                        }
                        this.apiError = null;
                    } else {
                        throw new Error(`Valor de predicción manual inválido: "${response.data.Prediccion}".`);
                    }
                } else {
                    throw new Error('Respuesta inválida (predicción manual).');
                }
            } catch (error) {
                console.error('Error triggerManualPrediction:', error);
                this.manualPredictionError = `Error al predecir: ${error.response?.data?.error || error.response?.data?.Message || error.message || 'Desconocido'}`;
            } finally {
                this.isPredictingManually = false;
            }
        }
    },
    mounted() {
        // ... (mounted logic remains the same) ...
        console.log('Component mounted');
        this.fetchPredictionData();
        this.fetchMonitoredList();
        console.log('Initial UserId from store:', this.userId);
        if (!this.userId) {
            console.warn('UserId not available on mount.');
        }
    }
};
</script>

<template>
    <div>
        <div class="health-monitor">
            <div class="face-container">
                <div v-if="isLoading && apiRiskPercentage === null" class="loading-indicator">Cargando datos iniciales...</div>
                <div v-else-if="apiError && apiRiskPercentage === null" class="error-message">
                    <p>Error al cargar datos iniciales:</p>
                    <p>{{ apiError }}</p>
                    <img :src="faceImage(null)" class="h-60 w-auto" alt="Error Face" style="opacity: 0.5" />
                </div>
                <div v-else>
                    <div v-if="isPredictingManually" class="loading-indicator small-loading">Actualizando predicción...</div>
                    <div v-if="manualPredictionError" class="error-message small-error">
                        <p>{{ manualPredictionError }}</p>
                    </div>
                    <img :src="faceImage(displayPercentage)" class="h-60 w-auto" alt="Risk Face" />
                    <div class="risk-info">
                        <div class="risk-label">{{ riskMessage }}:</div>
                        <div class="risk-percentage">{{ displayPercentage }}%</div>
                    </div>
                </div>
            </div>
            <div class="recommendations" :class="{ 'single-column': recommendations.length === 1 }">
                <div v-if="!isLoading && !apiError && !recommendations.length && !isPredictingManually" class="no-recommendations">
                    {{ apiRiskPercentage !== null ? 'No hay alertas específicas.' : 'Esperando resultado...' }}
                </div>
                <div v-else-if="!isLoading && !isPredictingManually && recommendations.length > 0" v-for="(alert, index) in recommendations" :key="'alert-' + index" class="recommendation-box">
                    {{ alert }}
                </div>
                <div v-else-if="isPredictingManually" class="no-recommendations">Actualizando alertas...</div>
            </div>
        </div>

        <div class="button-controls">
            <button @click="toggleCarousel" class="toggle-btn rounded-border">
                {{ isInfinite ? 'Modo Estático' : 'Modo Infinito' }}
            </button>
            <button @click="triggerManualPrediction" class="predict-btn rounded-border" :disabled="isPredictingManually || !userId">
                <span v-if="isPredictingManually">Prediciendo...</span>
                <span v-else>Predecir Ahora</span>
            </button>
        </div>
        <div v-if="!userId && !isLoading" class="user-id-missing-warning">No se puede predecir manualmente sin un ID de usuario cargado.</div>

        <div v-if="isListLoading" class="list-loading-indicator">Cargando lista de monitorizados...</div>
        <div v-else-if="listApiError" class="list-error-message">
            <p>Error al obtener la lista:</p>
            <p>{{ listApiError }}</p>
        </div>
        <div v-else-if="!monitoredPeople.length" class="no-monitored-people">No hay personas monitorizadas para mostrar.</div>
        <div v-else class="people-list">
            <div class="people-container" :class="{ 'infinite-scroll': isInfinite && monitoredPeople.length > 0 }" :style="[staticStyle, infiniteScrollStyles]">
                <div v-for="(person, index) in monitoredPeople" :key="'monitored-' + index" class="person-card">
                    <img :src="faceImage(person.percentage)" :alt="`Cara de ${person.name}`" class="h-auto w-40" />
                    <p>{{ person.name }}</p>
                    <p>{{ person.percentage }}%</p>
                </div>
            </div>
        </div>

        <div v-if="!isInfinite && monitoredPeople.length > 0" class="arrow-controls">
            <button @click="prevSlide" class="arrow-btn rounded-border">←</button>
            <button @click="nextSlide" class="arrow-btn rounded-border">→</button>
        </div>
    </div>
</template>

<style scoped>
/* ... (other styles remain the same - list-loading, error messages, health-monitor, face-container, risk-info, buttons, recommendations etc.) ... */

.list-loading-indicator,
.list-error-message,
.no-monitored-people {
    /* Styles are identical */
    text-align: center;
    margin: 2rem 1rem;
    font-weight: bold;
    color: #555;
}
.list-error-message {
    /* Styles are identical */
    color: red;
    border: 1px solid red;
    padding: 1rem;
    border-radius: 5px;
    background-color: #ffebee;
    width: 80%;
    max-width: 500px;
    margin-left: auto;
    margin-right: auto;
}
.loading-indicator,
.error-message,
.no-recommendations {
    /* Styles are identical */
    margin-top: 2rem;
    font-weight: bold;
    text-align: center;
}
.error-message {
    /* Styles are identical */
    color: red;
    border: 1px solid red;
    padding: 1rem;
    border-radius: 5px;
    background-color: #ffebee;
    width: 80%;
    max-width: 500px;
    margin-left: auto;
    margin-right: auto;
}
.loading-indicator.small-loading,
.error-message.small-error {
    /* Styles are identical */
    font-size: 0.9em;
    margin-bottom: 0.5rem;
    padding: 0.2rem;
}
.error-message.small-error {
    max-width: 400px;
    padding: 0.5rem;
}
.health-monitor {
    /* Styles are identical */
    display: flex;
    flex-direction: column;
    align-items: center;
    margin-bottom: 1rem;
}
.face-container {
    /* Styles are identical */
    text-align: center;
    min-height: 280px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    margin-bottom: 1rem;
}
.risk-info {
    /* Styles are identical */
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 1rem;
    gap: 0.5rem;
}
.risk-label,
.risk-percentage {
    font-size: 1.2rem;
    font-weight: bold;
}
.user-id-missing-warning {
    text-align: center;
    color: #b87f00;
    font-size: 0.9em;
    margin-top: 0.5rem;
}

.button-controls {
    /* Styles are identical */
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 1rem;
    margin-top: 1rem;
    flex-wrap: wrap;
}
.toggle-btn,
.predict-btn,
.arrow-btn {
    /* Styles are identical */
    padding: 0.6rem 1.2rem;
    background: #007bff;
    color: #fff;
    border: none;
    cursor: pointer;
    border-radius: 20px;
    font-weight: bold;
    transition:
        background-color 0.2s ease,
        opacity 0.2s ease;
}
.toggle-btn:hover,
.predict-btn:hover,
.arrow-btn:hover {
    background: #0056b3;
}
.predict-btn:disabled {
    background: #cccccc;
    cursor: not-allowed;
    opacity: 0.7;
}
.arrow-controls {
    /* Styles are identical */
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 1rem;
}

.recommendations {
    /* Styles are identical */
    margin-top: 1rem;
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 1rem;
    width: 100%;
    max-width: 800px;
    padding: 0 1rem;
    box-sizing: border-box;
    min-height: 50px;
}
.recommendations.single-column {
    flex-direction: column;
    align-items: center;
}
.recommendations .recommendation-box {
    /* Styles are identical */
    background-color: white;
    border: 2px solid #3498db;
    border-radius: 10px;
    color: #3498db;
    font-size: 1rem;
    padding: 1rem;
    text-align: center;
    flex-basis: 45%;
    box-sizing: border-box;
    margin: 0;
    min-height: 60px;
    display: flex;
    align-items: center;
    justify-content: center;
}
.recommendations.single-column .recommendation-box {
    flex-basis: 80%;
    width: auto;
}
.rounded-border {
    border-radius: 20px;
}

/* --- Carousel / People List Styles --- */
.people-list {
    width: 100%;
    overflow: hidden;
    position: relative;
    border-top: 1px solid #eee;
    padding-top: 1rem;
    margin-top: 1rem;
    white-space: nowrap; /* Keep items in a single line */
}

.people-list {
    width: 100%;
    overflow: hidden; /* Crucial: oculta lo que no cabe */
    position: relative;
    border-top: 1px solid #eee;
    padding-top: 1rem;
    margin-top: 1rem;
    white-space: nowrap; /* Crucial: evita que los inline-block salten de línea */
}

.people-container {
    /* display: block; */ /* Cambiado */
    display: inline-block; /* Clave: hace que el contenedor se ajuste al ancho de su contenido */
    /* CSS variables for animation - defaults set here or via :style */
    --scroll-width: 0px;
    --scroll-duration: 30s;
    transition: transform 0.5s ease; /* For static mode sliding */
    transform: translateX(0); /* Default starting position */
    will-change: transform; /* Optimization hint for animation */
    /* Eliminamos el white-space: nowrap; de aquí, ya está en el padre */
    /* SIN width explícito: el ancho será la suma de todos los .person-card (originales + duplicados) */
}

.person-card {
    /* flex: 0 0 auto; /* No se usa flex aquí */
    text-align: center;
    margin-right: 2rem; /* ~32px */
    width: 160px;
    display: inline-block; /* Clave: hace que las tarjetas se pongan una al lado de la otra */
    vertical-align: top; /* Alinea las tarjetas por la parte superior */
}

.person-card img {
    display: block;
    margin: 0 auto 0.5rem auto;
}

/* --- Infinite Scroll Animation --- */
/* Define the keyframes using CSS variables */
@keyframes scrollLeft {
    0% {
        transform: translateX(70vw); /* Comienza en el centro de la pantalla */
    }
    100% {
        /* Mueve el contenedor hacia la izquierda exactamente la anchura del set ORIGINAL */
        transform: translateX(var(--scroll-width));
    }
}

/* Apply animation ONLY when the class is present */
.people-container.infinite-scroll {
    /* ¡ELIMINADO! width: calc(var(--scroll-width) * -2); */
    animation: scrollLeft var(--scroll-duration) linear infinite;
    /* Static style transform is overridden by animation */
    transition: none; /* Disable transition when animating */
}
</style>
