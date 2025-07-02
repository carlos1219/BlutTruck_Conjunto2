<script setup>
import axios from 'axios';
import { computed, ref } from 'vue';
import { useUserStore } from '/stores/user'; // Asegúrate que la ruta a tu store sea correcta

const userStore = useUserStore();

// Definición de los campos del formulario
const form = ref({
    steps: null,
    activeCalories: null,
    heartRateData: null, // Para una entrada puntual de Frecuencia Cardíaca (bpm)
    restingHeartRate: null,
    weight: null,
    height: null,
    bloodPressureSystolic: null,
    bloodPressureDiastolic: null,
    bloodGlucose: null,
    bodyTemperature: null, // Para una entrada puntual de Temperatura Corporal
    respiratoryRate: null, // Para una entrada puntual de Frecuencia Respiratoria
    oxygenSaturation: null, // Para una entrada puntual de Saturación de Oxígeno
    date: null, // Fecha para la entrada de datos
    time: null // Hora para la entrada de datos
});

// Mensajes de error (sin cambios, tu lógica original)
const errors = ref({
    steps: '',
    activeCalories: '',
    heartRateData: '',
    restingHeartRate: '',
    weight: '',
    height: '',
    bloodPressureSystolic: '',
    bloodPressureDiastolic: '',
    bloodGlucose: '',
    bodyTemperature: '',
    respiratoryRate: '',
    oxygenSaturation: '',
    date: '',
    time: ''
});

// Función para formatear la fecha en YYYY-MM-DD (sin cambios)
const formatDate = (dateString) => {
    if (!dateString) return null;
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

// Computed para determinar si el formulario está vacío (sin cambios)
const isFormEmpty = computed(() => Object.values(form.value).every((value) => value === null || value === ''));

// Función para validar el formulario (sin cambios, tu lógica original)
function validateForm() {
    let isValid = true;
    // Resetear errores
    Object.keys(errors.value).forEach((key) => (errors.value[key] = ''));

    const numericFields = ['steps', 'activeCalories', 'heartRateData', 'restingHeartRate', 'weight', 'height', 'bloodPressureSystolic', 'bloodPressureDiastolic', 'bloodGlucose', 'bodyTemperature', 'respiratoryRate', 'oxygenSaturation'];
    numericFields.forEach((field) => {
        if (form.value[field] !== null && form.value[field] !== '' && Number(form.value[field]) <= 0) {
            errors.value[field] = 'El valor debe ser mayor que 0.';
            isValid = false;
        }
    });
    if (form.value.bodyTemperature !== null && form.value.bodyTemperature !== '' && (form.value.bodyTemperature < 35 || form.value.bodyTemperature > 42)) {
        errors.value.bodyTemperature = 'La temperatura corporal debe estar entre 35°C y 42°C.';
        isValid = false;
    }
    if (form.value.oxygenSaturation !== null && form.value.oxygenSaturation !== '' && (form.value.oxygenSaturation < 70 || form.value.oxygenSaturation > 100)) {
        errors.value.oxygenSaturation = 'La saturación de oxígeno debe estar entre 70% y 100%.';
        isValid = false;
    }
    if (!form.value.date) {
        errors.value.date = 'La fecha es obligatoria.';
        isValid = false;
    }
    return isValid;
}

// Variables para mensajes de la API
const apiError = ref(null);
const apiSuccessMessage = ref(null);

async function handleSubmit() {
    apiError.value = null;
    apiSuccessMessage.value = null;

    if (isFormEmpty.value) {
        alert('Por favor, ingrese al menos un dato antes de guardar.');
        return;
    }
    if (!validateForm()) {
        alert('Por favor, corrija los errores en el formulario.');
        return;
    }

    // 1. CONSTRUIR EL TIMESTAMP PARA LA ENTRADA ACTUAL
    let entryTimestampIso;
    if (form.value.date && form.value.time) {
        entryTimestampIso = new Date(`${form.value.date}T${form.value.time}`).toISOString();
    } else if (form.value.date) {
        // Si solo hay fecha, usar la hora actual del día seleccionado
        const now = new Date();
        const dateParts = form.value.date.split('-');
        entryTimestampIso = new Date(
            parseInt(dateParts[0]),
            parseInt(dateParts[1]) - 1, // Mes es 0-indexado
            parseInt(dateParts[2]),
            now.getHours(),
            now.getMinutes(),
            now.getSeconds()
        ).toISOString();
    } else {
        // Esto no debería ocurrir si validateForm() funciona
        errors.value.date = 'La fecha es obligatoria.';
        return;
    }

    // 2. CONSTRUIR EL PAYLOAD PARA ACTUALIZAR EL USERSTORE (Pinia)
    //    Este payload debe usar claves en minúscula y estructuras de array para datos de series temporales,
    //    tal como lo espera la gráfica y como lo envía Flutter.
    const payloadForStore = {};

    // Campos que son valores únicos para el día (o el más reciente)
    if (form.value.steps != null && form.value.steps !== '') payloadForStore.steps = parseInt(form.value.steps);
    if (form.value.activeCalories != null && form.value.activeCalories !== '') payloadForStore.activeCalories = parseInt(form.value.activeCalories);
    if (form.value.restingHeartRate != null && form.value.restingHeartRate !== '') payloadForStore.restingHeartRate = parseInt(form.value.restingHeartRate);
    if (form.value.weight != null && form.value.weight !== '') payloadForStore.weight = parseFloat(form.value.weight);
    if (form.value.height != null && form.value.height !== '') payloadForStore.height = parseFloat(form.value.height);
    // BodyTemperature como valor único si también se necesita así (Flutter lo envía como el más reciente)
    if (form.value.bodyTemperature != null && form.value.bodyTemperature !== '') {
        payloadForStore.bodyTemperature = parseFloat(form.value.bodyTemperature);
    }

    // Campos que son series temporales (arrays de objetos)
    if (form.value.heartRateData != null && form.value.heartRateData !== '') {
        payloadForStore.heartRateData = [{ time: entryTimestampIso, bpm: parseInt(form.value.heartRateData) }];
    }
    if ((form.value.bloodPressureSystolic != null && form.value.bloodPressureSystolic !== '') || (form.value.bloodPressureDiastolic != null && form.value.bloodPressureDiastolic !== '')) {
        const bpEntry = { time: entryTimestampIso };
        if (form.value.bloodPressureSystolic != null && form.value.bloodPressureSystolic !== '') bpEntry.systolic = parseInt(form.value.bloodPressureSystolic);
        if (form.value.bloodPressureDiastolic != null && form.value.bloodPressureDiastolic !== '') bpEntry.diastolic = parseInt(form.value.bloodPressureDiastolic);
        payloadForStore.bloodPressureData = [bpEntry];
    }
    if (form.value.bloodGlucose != null && form.value.bloodGlucose !== '') {
        payloadForStore.bloodGlucoseData = [{ time: entryTimestampIso, bloodGlucose: parseFloat(form.value.bloodGlucose) }];
    }
    if (form.value.bodyTemperature != null && form.value.bodyTemperature !== '') {
        // Para la gráfica
        payloadForStore.temperatureData = [{ time: entryTimestampIso, temperature: parseFloat(form.value.bodyTemperature) }];
    }
    if (form.value.respiratoryRate != null && form.value.respiratoryRate !== '') {
        payloadForStore.respiratoryRateData = [{ time: entryTimestampIso, rate: parseInt(form.value.respiratoryRate) }];
    }
    if (form.value.oxygenSaturation != null && form.value.oxygenSaturation !== '') {
        payloadForStore.oxygenSaturationData = [{ time: entryTimestampIso, percentage: parseInt(form.value.oxygenSaturation) }];
    }
    // Nota: SleepData es más complejo y no se captura en este formulario simple.

    // 3. ACTUALIZAR EL USERSTORE (Pinia) LOCALMENTE
    const formattedDateKey = formatDate(form.value.date);
    const currentDias = JSON.parse(JSON.stringify(userStore.fullUserData?.dias || {})); // Copia profunda

    if (!currentDias[formattedDateKey]) {
        currentDias[formattedDateKey] = {}; // Inicializar el día si no existe
    }
    const existingDataForDay = currentDias[formattedDateKey];

    for (const key in payloadForStore) {
        if (Array.isArray(payloadForStore[key])) {
            // Concatenar arrays (para series temporales)
            existingDataForDay[key] = [...(existingDataForDay[key] || []), ...payloadForStore[key]];
        } else {
            // Sobrescribir valores individuales
            existingDataForDay[key] = payloadForStore[key];
        }
    }
    userStore.updateDiasInUserData(currentDias); // Asume que esta función actualiza userStore.fullUserData.dias

    // 4. CONSTRUIR Y ENVIAR EL PAYLOAD A LA API
    //    Este payload debe coincidir con lo que tu backend C# espera (posiblemente claves en mayúsculas).
    try {
        const userId = userStore.fullUserData?.userId;
        if (!userId) {
            throw new Error('No se pudo obtener el ID del usuario.');
        }

        const healthDataForApi = {
            userId: userId, // Flutter usa "userId"
            steps: payloadForStore.steps,
            activeCalories: payloadForStore.activeCalories,
            restingHeartRate: payloadForStore.restingHeartRate,
            weight: payloadForStore.weight,
            height: payloadForStore.height,

            bodyTemperature: form.value.bodyTemperature != null && form.value.bodyTemperature !== '' ? parseFloat(form.value.bodyTemperature) : null,

            heartRateData: existingDataForDay.heartRateData || [],
            bloodPressureData: existingDataForDay.bloodPressureData || [],
            bloodGlucoseData: existingDataForDay.bloodGlucoseData || [],
            temperatureData: existingDataForDay.temperatureData || [],
            respiratoryRateData: existingDataForDay.respiratoryRateData || [],
            oxygenSaturationData: existingDataForDay.oxygenSaturationData || [],
            // SleepData no se maneja en este formulario, se enviaría vacío o se omitiría.
            sleepData: existingDataForDay.sleepData || []
        };

        // Eliminar propiedades nulas o vacías de healthDataForApi si el backend no las maneja bien
        for (const key in healthDataForApi) {
            if (healthDataForApi[key] === null || healthDataForApi[key] === undefined || (Array.isArray(healthDataForApi[key]) && healthDataForApi[key].length === 0)) {
                // Opcional: decide si enviar array vacío o quitar la clave
                // delete healthDataForApi[key];
            }
        }

        const requestBodyForApi = {
            Credentials: {
                // Tu código Flutter usa "credentials" (minúscula)
                userId: userId,
                idToken: 'string' // Placeholder, tu backend Flutter lo maneja
            },
            healthData: healthDataForApi // Tu código Flutter usa "healthData" (minúscula)
        };

        console.log('Enviando Payload a API (writeWeb):', JSON.stringify(requestBodyForApi, null, 2));
        const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/writeWeb'; // Reemplaza con tu URL real
        const response = await axios.post(apiUrl, requestBodyForApi);

        if (response.status === 200 || response.status === 201) {
            apiSuccessMessage.value = 'Datos de salud guardados exitosamente en la API.';
            console.log('Respuesta de la API (writeWeb):', response.data);

            // Resetear solo los campos de valor, no la fecha/hora para facilitar múltiples entradas para el mismo momento
            Object.keys(form.value).forEach((key) => {
                if (key !== 'date' && key !== 'time') {
                    form.value[key] = null;
                }
            });
            // Opcional: Forzar recarga de datos desde el backend para máxima consistencia
            // await userStore.fetchFullUserData();
        } else {
            throw new Error(`Error ${response.status} desde API (writeWeb): ${JSON.stringify(response.data)}`);
        }
    } catch (error) {
        apiError.value = `Error en la solicitud a la API (writeWeb): ${error.response?.data?.message || error.response?.data || error.message}`;
        console.error('Error en la solicitud a la API (writeWeb):', error.response || error);
    }
}
</script>

<template>
    <div class="p-6 bg-surface-100 dark:bg-surface-900 min-h-screen flex items-center justify-center">
        <div class="bg-white dark:bg-surface-800 p-8 rounded-lg shadow-xl w-full max-w-2xl">
            <h2 class="text-3xl font-bold mb-8 text-center text-gray-800 dark:text-gray-100">Registro Manual de Datos de Salud</h2>

            <form @submit.prevent="handleSubmit" class="space-y-6">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <label for="date" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Fecha de la Medición</label>
                        <input
                            id="date"
                            v-model="form.date"
                            type="date"
                            required
                            class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 dark:bg-surface-700 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
                        />
                        <span v-if="errors.date" class="text-red-500 text-xs mt-1">{{ errors.date }}</span>
                    </div>
                    <div>
                        <label for="time" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Hora de la Medición (Opcional)</label>
                        <input
                            id="time"
                            v-model="form.time"
                            type="time"
                            class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 dark:bg-surface-700 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
                        />
                        <span v-if="errors.time" class="text-red-500 text-xs mt-1">{{ errors.time }}</span>
                    </div>
                </div>
                <hr class="dark:border-gray-700" />

                <div class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                    <div>
                        <label for="heartRateData" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Frecuencia Cardíaca (bpm)</label>
                        <input id="heartRateData" v-model="form.heartRateData" type="number" placeholder="Ej: 70" class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white" />
                        <span v-if="errors.heartRateData" class="text-red-500 text-xs">{{ errors.heartRateData }}</span>
                    </div>
                    <div>
                        <label for="restingHeartRate" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Frec. Cardíaca en Reposo (bpm)</label>
                        <input id="restingHeartRate" v-model="form.restingHeartRate" type="number" placeholder="Ej: 60" class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white" />
                        <span v-if="errors.restingHeartRate" class="text-red-500 text-xs">{{ errors.restingHeartRate }}</span>
                    </div>

                    <div>
                        <label for="bloodPressureSystolic" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Presión Sistólica (mmHg)</label>
                        <input
                            id="bloodPressureSystolic"
                            v-model="form.bloodPressureSystolic"
                            type="number"
                            placeholder="Ej: 120"
                            class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white"
                        />
                        <span v-if="errors.bloodPressureSystolic" class="text-red-500 text-xs">{{ errors.bloodPressureSystolic }}</span>
                    </div>
                    <div>
                        <label for="bloodPressureDiastolic" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Presión Diastólica (mmHg)</label>
                        <input
                            id="bloodPressureDiastolic"
                            v-model="form.bloodPressureDiastolic"
                            type="number"
                            placeholder="Ej: 80"
                            class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white"
                        />
                        <span v-if="errors.bloodPressureDiastolic" class="text-red-500 text-xs">{{ errors.bloodPressureDiastolic }}</span>
                    </div>
                    <div>
                        <label for="oxygenSaturation" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Saturación de Oxígeno (%)</label>
                        <input id="oxygenSaturation" v-model="form.oxygenSaturation" type="number" placeholder="Ej: 98" class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white" />
                        <span v-if="errors.oxygenSaturation" class="text-red-500 text-xs">{{ errors.oxygenSaturation }}</span>
                    </div>
                    <div>
                        <label for="bodyTemperature" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Temperatura Corporal (°C)</label>
                        <input
                            id="bodyTemperature"
                            v-model="form.bodyTemperature"
                            type="number"
                            step="0.1"
                            placeholder="Ej: 36.6"
                            class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white"
                        />
                        <span v-if="errors.bodyTemperature" class="text-red-500 text-xs">{{ errors.bodyTemperature }}</span>
                    </div>
                    <div>
                        <label for="steps" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Pasos</label>
                        <input id="steps" v-model="form.steps" type="number" placeholder="Ej: 10000" class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white" />
                        <span v-if="errors.steps" class="text-red-500 text-xs">{{ errors.steps }}</span>
                    </div>
                    <div>
                        <label for="activeCalories" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Calorías Activas (kcal)</label>
                        <input id="activeCalories" v-model="form.activeCalories" type="number" placeholder="Ej: 300" class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white" />
                        <span v-if="errors.activeCalories" class="text-red-500 text-xs">{{ errors.activeCalories }}</span>
                    </div>

                    <div>
                        <label for="bloodGlucose" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Glucosa en Sangre (mg/dL)</label>
                        <input
                            id="bloodGlucose"
                            v-model="form.bloodGlucose"
                            type="number"
                            step="0.1"
                            placeholder="Ej: 90.0"
                            class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white"
                        />
                        <span v-if="errors.bloodGlucose" class="text-red-500 text-xs">{{ errors.bloodGlucose }}</span>
                    </div>
                    <div>
                        <label for="respiratoryRate" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Frec. Respiratoria (rpm)</label>
                        <input id="respiratoryRate" v-model="form.respiratoryRate" type="number" placeholder="Ej: 16" class="mt-1 p-3 w-full border border-gray-300 dark:border-gray-600 rounded-md shadow-sm dark:bg-surface-700 dark:text-white" />
                        <span v-if="errors.respiratoryRate" class="text-red-500 text-xs">{{ errors.respiratoryRate }}</span>
                    </div>
                </div>
                <hr class="dark:border-gray-700 mt-8 mb-6" />

                <div v-if="apiSuccessMessage" class="mt-4 p-3 bg-green-100 dark:bg-green-700 border border-green-400 dark:border-green-600 text-green-700 dark:text-green-100 rounded-md">
                    {{ apiSuccessMessage }}
                </div>
                <div v-if="apiError" class="mt-4 p-3 bg-red-100 dark:bg-red-700 border border-red-400 dark:border-red-600 text-red-700 dark:text-red-100 rounded-md">
                    {{ apiError }}
                </div>

                <div>
                    <button
                        type="submit"
                        class="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold p-3 rounded-md shadow-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 dark:focus:ring-offset-gray-800 transition ease-in-out duration-150"
                        :disabled="isFormEmpty && !form.date"
                    >
                        Guardar Datos
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>

<style scoped>
/* Estilos adicionales si son necesarios */
input:required:invalid {
    border-color: red; /* Ejemplo de estilo para campos requeridos inválidos */
}
</style>
