<script setup>
import axios from 'axios'; // Importa Axios
import Calendar from 'primevue/calendar';
import Slider from 'primevue/slider';
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';
import { useUserStore } from '/stores/user';
// Usar el hook para obtener el servicio de confirmación

// Acceder al store de Pinia
const userStore = useUserStore();
const profile = ref({
    name: '',
    weight: null,
    height: null,
    dateOfBirth: null,
    gender: '',
    smoke: false,
    alcohol: false,
    cholesterol: 0,
    hasPredisposition: false,
    imageUrl: '/demo/images/monitoreo/hombre1.jpg', // URL de imagen de ejemplo por defecto
    connectionType: 'normal', // Tipo de conexión por defecto
    notificationPercentage: 60, // Valor por defecto para el porcentaje de alerta
    connectionCode: null,
    connectionCodeInput: null
});

const toast = useToast();

// Función para actualizar el ref profile con los datos del store
const updateProfileFromStore = () => {
    // Define la imagen por defecto aquí para fácil acceso
    const defaultImageUrl = '/demo/images/monitoreo/hombre1.png';

    if (userStore.monitoredUserData && userStore.monitoredUserData.datosPersonales) {
        const datos = userStore.monitoredUserData.datosPersonales;
        profile.value.name = datos.name || '';
        profile.value.weight = datos.weight || null;
        profile.value.height = datos.height || null;
        profile.value.dateOfBirth = datos.dateOfBirth ? new Date(datos.dateOfBirth) : null;
        profile.value.gender = datos.gender === 1 ? 'Hombre' : datos.gender === 2 ? 'Mujer' : datos.gender === 3 ? 'Otro' : '';
        profile.value.smoke = !!datos.smoke;
        profile.value.alcohol = !!datos.alcohol;
        profile.value.cholesterol = datos.choresterol || datos.cholesterol || 0;
        profile.value.hasPredisposition = !!datos.hasPredisposition;

        profile.value.imageUrl = datos.photoURL || defaultImageUrl;
    } else {
        profile.value.imageUrl = defaultImageUrl;
    }
};

// Función para validar que los datos sean realistas
function validateProfile() {
    if (!profile.value.name || profile.value.name.trim().length === 0) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'El nombre es obligatorio', life: 3000 });
        return false;
    }
    if (!profile.value.weight || profile.value.weight < 30 || profile.value.weight > 300) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Peso no realista (debe estar entre 30 y 300 kg)', life: 3000 });
        return false;
    }
    if (!profile.value.height || profile.value.height < 100 || profile.value.height > 250) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Altura no realista (debe estar entre 100 y 250 cm)', life: 3000 });
        return false;
    }
    if (!profile.value.dateOfBirth) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'La fecha de nacimiento es obligatoria', life: 3000 });
        return false;
    }
    // Puedes agregar más validaciones según el campo
    return true;
}

onMounted(() => {
    updateProfileFromStore();
    // Ejemplo: if (!userStore.monitoredUserData) { fetchUserData(); }
});

async function saveProfile() {
    // Primero validamos los datos
    if (!validateProfile()) {
        return; // Se detiene el guardado si hay error en la validación
    }

    const userId = userStore.monitoredUserData.monitoredUserId;
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/save-profile';

    const profileDataToSend = {
        Credentials: {
            UserId: userId,
            IdToken: 'string'
        },
        profile: {
            Conexion: {
                ConnectionStatus: 0
            },
            DateOfBirth: profile.value.dateOfBirth,
            HasPredisposition: profile.value.hasPredisposition,
            Height: parseInt(profile.value.height) || null,
            Weight: parseInt(profile.value.weight) || null,
            Gender: profile.value.gender === 'Hombre' ? 1 : profile.value.gender === 'Mujer' ? 2 : profile.value.gender === 'Otro' ? 3 : null,
            Smoke: profile.value.smoke ? 1 : 0,
            Alcohol: profile.value.alcohol ? 1 : 0,
            Choresterol: profile.value.cholesterol,
            PhotoURL: profile.value.imageUrl || null,
            Name: profile.value.name || null,
            Active: true
        }
    };

    try {
        const response = await axios.post(apiUrl, profileDataToSend);

        if (response.status === 200) {
            toast.add({ severity: 'success', summary: 'Éxito', detail: 'Perfil guardado correctamente', life: 3000 });
            console.log('Perfil guardado en el backend', response.data);
            // Supongamos que la respuesta incluye los datos actualizados del perfil:
            if (response.data && response.data.datosPersonales) {
                userStore.monitoredUserData.datosPersonales = { ...response.data.datosPersonales };
            } else {
                // Como alternativa, actualizamos el store con los valores locales:
                userStore.monitoredUserData.datosPersonales = {
                    name: profile.value.name,
                    weight: profile.value.weight,
                    height: profile.value.height,
                    dateOfBirth: profile.value.dateOfBirth,
                    gender: profile.value.gender === 'Hombre' ? 1 : profile.value.gender === 'Mujer' ? 2 : profile.value.gender === 'Otro' ? 3 : null,
                    smoke: profile.value.smoke,
                    alcohol: profile.value.alcohol,
                    choresterol: profile.value.cholesterol,
                    hasPredisposition: profile.value.hasPredisposition,
                    imageUrl: profile.value.imageUrl
                };
            }
        } else {
            toast.add({ severity: 'error', summary: 'Error', detail: `Error al guardar el perfil: ${response.data?.message || 'Error desconocido'}`, life: 3000 });
            console.error('Error al guardar el perfil:', response.status, response.data);
        }
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Error', detail: `Error al guardar el perfil: ${error.message || error}`, life: 3000 });
        console.error('Error al guardar el perfil:', error);
        if (error.response) {
            console.error('Datos de la respuesta:', error.response.data);
            console.error('Estado de la respuesta:', error.response.status);
            console.error('Cabeceras de la respuesta:', error.response.headers);
        } else if (error.request) {
            console.error('Datos de la solicitud:', error.request);
        } else {
            console.error('Error al configurar la solicitud:', error.message);
        }
    }
}

async function downloadPdf() {
    // URL del endpoint, ajústala según la configuración de tu API
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/ReadData/get-pdf';
    const payload = {
        Credentials: {
            UserId: userStore.monitoredUserData.monitoredUserId,
            IdToken: 'string'
        }
    };

    try {
        // Importante: se configura responseType: 'blob' para manejar archivos binarios
        const response = await axios.post(apiUrl, payload, { responseType: 'blob' });
        // Se crea el blob a partir de la respuesta
        const blob = new Blob([response.data], { type: 'application/pdf' });
        // Se genera una URL temporal para el blob
        const url = window.URL.createObjectURL(blob);
        // Se crea un enlace (elemento 'a') y se simula un clic para descargar el archivo
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', 'datosSalud.pdf');
        document.body.appendChild(link);
        link.click();
        // Limpieza: se revoca la URL y se elimina el enlace
        window.URL.revokeObjectURL(url);
        document.body.removeChild(link);
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Error', detail: error.message });
        console.error('Error al descargar el PDF:', error);
    }
}
</script>

<template>
    <div class="p-6 bg-surface-100 dark:bg-surface-900 min-h-screen flex items-center justify-center">
        <ConfirmDialog />
        <Toast />
        <div class="bg-white dark:bg-surface-800 p-8 rounded-lg shadow-lg w-full max-w-4xl grid grid-cols-1 md:grid-cols-2 gap-8">
            <div class="flex flex-col items-center md:items-start">
                <h2 class="text-3xl font-bold mb-6 text-center md:text-left text-gray-800 dark:text-gray-200">Bienvenido, {{ profile.name }}</h2>
                <div class="mb-6">
                    <img :src="profile.imageUrl" alt="Foto de perfil" class="w-32 h-32 rounded-full object-cover" />
                </div>
                <!-- Checkbox y slider permanecen iguales -->
                <div class="mb-4 flex items-center">
                    <input type="checkbox" v-model="profile.smoke" class="mt-1 p-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                    <label class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Fumador</label>
                </div>
                <div class="mb-4 flex items-center">
                    <input type="checkbox" v-model="profile.alcohol" class="mt-1 p-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                    <label class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Bebe Alcohol</label>
                </div>
                <div class="mb-4 flex items-center">
                    <input type="checkbox" v-model="profile.hasPredisposition" class="mt-1 p-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                    <label class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Predisposición</label>
                </div>
                <div class="mb-4 w-full">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Nivel de Colesterol</label>
                    <Slider v-model="profile.cholesterol" :min="1" :max="5" :step="1" class="w-full my-slider" />
                    <div class="text-xs text-gray-600 mt-1">{{ profile.cholesterol * 20 }} mg/dL</div>
                </div>
            </div>

            <div class="flex flex-col">
                <div class="mb-4">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Nombre</label>
                    <input type="text" v-model="profile.name" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Peso (kg)</label>
                    <input type="number" v-model="profile.weight" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Altura (cm)</label>
                    <input type="number" v-model="profile.height" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Fecha de Nacimiento</label>
                    <Calendar v-model="profile.dateOfBirth" dateFormat="yy-mm-dd" showIcon class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Género</label>
                    <select v-model="profile.gender" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-600 dark:border-surface-600 dark:text-white">
                        <option value="Hombre">Hombre</option>
                        <option value="Mujer">Mujer</option>
                        <option value="Otro">Otro</option>
                    </select>
                </div>
                <div class="mb-4">
                    <button @click="saveProfile" class="w-full bg-blue-500 hover:bg-blue-600 text-white p-3 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-blue-700 dark:hover:bg-blue-800">Guardar</button>
                </div>
            </div>

            <div class="col-span-1 md:col-span-2 flex justify-center mt-4 space-x-4">
                <button @click="downloadPdf" class="bg-purple-500 hover:bg-purple-100 text-white p-3 rounded-md shadow-sm focus:ring-purple-500 focus:border-purple-100 dark:bg-purple-900 dark:hover:bg-purple-950">Obtener Todos los Datos</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
:deep(.my-slider .p-slider-range) {
    background-color: #3498db !important;
}
:deep(.my-slider .p-slider-handle) {
    background-color: #3498db !important;
    border-color: #3498db !important;
}
</style>
