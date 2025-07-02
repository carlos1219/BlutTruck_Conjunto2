<script setup>
import axios from 'axios'; // Importa Axios
import Calendar from 'primevue/calendar';
import Dialog from 'primevue/dialog'; // Asegúrate que Dialog esté importado
import Slider from 'primevue/slider';
import { useConfirm } from 'primevue/useconfirm'; // Importar el hook useConfirm
import { useToast } from 'primevue/usetoast';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '/stores/user'; // Asegúrate que la ruta sea correcta

// Usar el hook para obtener el servicio de confirmación
const confirm = useConfirm();
const toast = useToast();

// Acceder al store de Pinia
const userStore = useUserStore();
const router = useRouter();

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
    connectionCode: null,
    connectionCodeInput: null
});

// --- Nuevos refs para el diálogo de selección de tipo de dato PDF ---
const isMoreOptionsDialogOpen = ref(false);
const isDataTypeSelectDialogOpen = ref(false); // Para el nuevo diálogo

// Opciones para el PDF por tipo de dato, basadas en el enum HealthDataType de C#
// Los valores (0, 1, 2...) deben coincidir con los valores enteros del enum en C#
const healthDataTypeOptions = ref([
    { label: 'Pasos', value: 0, fileNamePart: 'Pasos' },
    { label: 'Ritmo Cardíaco (Detallado)', value: 1, fileNamePart: 'RitmoCardiaco' },
    { label: 'Temperatura Corporal (Mediciones)', value: 2, fileNamePart: 'TemperaturaMediciones' },
    { label: 'Calorías Activas', value: 3, fileNamePart: 'CaloriasActivas' },
    { label: 'Glucosa en Sangre', value: 4, fileNamePart: 'GlucosaSangre' },
    { label: 'Presión Arterial', value: 5, fileNamePart: 'PresionArterial' },
    { label: 'Saturación de Oxígeno', value: 6, fileNamePart: 'SaturacionOxigeno' },
    { label: 'Frecuencia Respiratoria', value: 7, fileNamePart: 'FrecuenciaRespiratoria' },
    { label: 'Sueño', value: 8, fileNamePart: 'Sueno' },
    { label: 'Ritmo Cardíaco Promedio (Diario)', value: 9, fileNamePart: 'RitmoCardiacoPromedio' },
    { label: 'Temperatura Corporal (General Diario)', value: 10, fileNamePart: 'TemperaturaGeneral' },
    { label: 'Ritmo Cardíaco Máximo (Diario)', value: 11, fileNamePart: 'RitmoCardiacoMax' },
    { label: 'Ritmo Cardíaco Mínimo (Diario)', value: 12, fileNamePart: 'RitmoCardiacoMin' },
    { label: 'Ritmo Cardíaco en Reposo (Diario)', value: 13, fileNamePart: 'RitmoCardiacoReposo' }
]);
// --- Fin de nuevos refs ---

// Función para actualizar el ref profile con los datos del store
const updateProfileFromStore = () => {
    const defaultImageUrl = '/demo/images/monitoreo/hombre1.png'; // Ruta a tu imagen por defecto

    if (userStore.fullUserData && userStore.fullUserData.datosPersonales) {
        const datos = userStore.fullUserData.datosPersonales;
        profile.value.name = datos.name || '';
        profile.value.weight = datos.weight || null;
        profile.value.height = datos.height || null;
        profile.value.dateOfBirth = datos.dateOfBirth ? new Date(datos.dateOfBirth) : null;
        profile.value.gender = datos.gender === 1 ? 'Hombre' : datos.gender === 2 ? 'Mujer' : datos.gender === 3 ? 'Otro' : '';
        profile.value.smoke = !!datos.smoke; // Asegura que sea booleano
        profile.value.alcohol = !!datos.alcohol; // Asegura que sea booleano
        profile.value.cholesterol = datos.choresterol || datos.cholesterol || 0; // Maneja ambos nombres de propiedad
        profile.value.hasPredisposition = !!datos.hasPredisposition; // Asegura que sea booleano
        profile.value.imageUrl = datos.photoURL || defaultImageUrl;
    } else {
        profile.value.name = 'Usuario';
        profile.value.imageUrl = defaultImageUrl;
    }
};

onMounted(() => {
    updateProfileFromStore();
});

function openMoreOptionsDialog() {
    isMoreOptionsDialogOpen.value = true;
}

function openDataTypeSelectDialog() {
    isMoreOptionsDialogOpen.value = false;
    isDataTypeSelectDialogOpen.value = true;
}

function validateProfile() {
    if (!profile.value.name || profile.value.name.trim().length === 0) {
        toast.add({ severity: 'error', summary: 'Error de Validación', detail: 'El nombre es obligatorio.', life: 3000 });
        return false;
    }
    if (profile.value.weight !== null && (profile.value.weight < 20 || profile.value.weight > 300)) {
        toast.add({ severity: 'error', summary: 'Error de Validación', detail: 'Peso no realista (debe estar entre 20 y 300 kg).', life: 3000 });
        return false;
    }
    if (profile.value.height !== null && (profile.value.height < 100 || profile.value.height > 250)) {
        toast.add({ severity: 'error', summary: 'Error de Validación', detail: 'Altura no realista (debe estar entre 100 y 250 cm).', life: 3000 });
        return false;
    }
    if (!profile.value.dateOfBirth) {
        toast.add({ severity: 'error', summary: 'Error de Validación', detail: 'La fecha de nacimiento es obligatoria.', life: 3000 });
        return false;
    }
    if (!profile.value.gender) {
        toast.add({ severity: 'error', summary: 'Error de Validación', detail: 'El género es obligatorio.', life: 3000 });
        return false;
    }
    return true;
}

async function saveProfile() {
    // Primero validamos los datos
    if (!validateProfile()) {
        return; // Se detiene el guardado si hay error en la validación
    }

    const userId = userStore.fullUserData.userId;
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
            photoURL: userStore.fullUserData.datosPersonales.photoURL || null,
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
                userStore.fullUserData.datosPersonales = { ...response.data.datosPersonales };
            } else {
                // Como alternativa, actualizamos el store con los valores locales:
                userStore.fullUserData.datosPersonales = {
                    name: profile.value.name,
                    weight: profile.value.weight,
                    height: profile.value.height,
                    dateOfBirth: profile.value.dateOfBirth,
                    gender: profile.value.gender === 'Hombre' ? 1 : profile.value.gender === 'Mujer' ? 2 : profile.value.gender === 'Otro' ? 3 : null,
                    smoke: profile.value.smoke,
                    alcohol: profile.value.alcohol,
                    choresterol: profile.value.cholesterol,
                    hasPredisposition: profile.value.hasPredisposition,
                    photoURL: userStore.fullUserData.datosPersonales.photoURL
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

function signOut() {
    userStore.$reset();
    toast.add({ severity: 'warn', summary: 'Cerrar Sesión', detail: 'Has cerrado sesión.', life: 3000 });
    router.push('/'); // Redirige a la página de inicio/login
}

function generateDeterministicCode(userId, type) {
    const input = `${userId}-${type}`;
    let hash = 5381;
    for (let i = 0; i < input.length; i++) {
        hash = (hash << 5) + hash + input.charCodeAt(i); /* hash * 33 + c */
    }
    return Math.abs(hash).toString(36).toUpperCase().substr(0, 8);
}

function generateConnectionCode(type) {
    const userId = userStore.fullUserData?.userId;
    if (!userId) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'ID de usuario no disponible.', life: 3000 });
        return;
    }
    profile.value.connectionType = type;
    profile.value.connectionCode = generateDeterministicCode(userId, type);
    registerConnectionCode();
    toast.add({
        severity: 'info',
        summary: 'Código Generado',
        detail: `Código de conexión (${type}): ${profile.value.connectionCode}`,
        life: 5000
    });
}

function generateConnectionCodeAdmin(type) {
    const userId = userStore.fullUserData?.userId;
    if (!userId) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'ID de usuario no disponible.', life: 3000 });
        return;
    }
    profile.value.connectionType = type;
    profile.value.connectionCode = generateDeterministicCode(userId, type);
    registerConnectionCodeAdmin();
    toast.add({
        severity: 'info',
        summary: 'Código Generado',
        detail: `Código de conexión (${type}): ${profile.value.connectionCode}`,
        life: 5000
    });
}

async function getConnectionUserId() {
    if (!profile.value.connectionCodeInput) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Introduce un código de conexión válido.', life: 3000 });
        return null;
    }
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/GetCodeConnection';
    const payload = {
        Code: profile.value.connectionCodeInput,
        IdToken: 'string'
    };
    try {
        const response = await axios.post(apiUrl, payload);
        // Asegúrate que la respuesta y la ruta al ID son correctas
        if (response.data && response.data.result && response.data.result.id) {
            return response.data.result.id;
        } else {
            toast.add({ severity: 'error', summary: 'Error', detail: 'Respuesta inesperada del servidor al obtener ID.', life: 3000 });
            return null;
        }
    } catch (error) {
        const errorMsg = error.response?.data?.Message || 'Error al obtener el ID del código de conexión.';
        toast.add({ severity: 'error', summary: 'Error', detail: errorMsg, life: 3000 });
        return null;
    }
}

async function connectWithCode() {
    if (!profile.value.connectionCodeInput) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'Introduce un código de conexión válido.', life: 3000 });
        return;
    }
    const connectedUserId = await getConnectionUserId();
    if (connectedUserId) {
        const registered = await registerConnection(connectedUserId);
        if (registered) {
            toast.add({ severity: 'success', summary: 'Conexión Establecida', detail: 'La conexión se ha registrado.', life: 3000 });
            await deleteConnectionCode(profile.value.connectionCodeInput); // Pasar el código usado
        }
    }
}

async function registerConnection(connectedUserId) {
    const currentUserId = userStore.fullUserData?.userId;
    if (!currentUserId) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'ID de usuario actual no disponible.', life: 3000 });
        return false;
    }
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/registerConnection';
    const payload = { CurrentUserId: currentUserId, ConnectedUserId: connectedUserId, IdToken: 'string' };
    try {
        await axios.post(apiUrl, payload);
        // toast.add({ severity: 'success', summary: 'Conexión Registrada', life: 3000 }); // Ya se muestra en connectWithCode
        return true;
    } catch (error) {
        const errorMsg = error.response?.data?.Message || 'Error al registrar la conexión.';
        toast.add({ severity: 'error', summary: 'Error de Registro', detail: errorMsg, life: 3000 });
        return false;
    }
}

async function registerConnectionCode() {
    const userId = userStore.fullUserData?.userId;
    if (!profile.value.connectionCode || !userId) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'No se ha generado código o falta ID de usuario.', life: 3000 });
        return;
    }
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/registerCodeConnection';
    const payload = { CurrentUserId: userId, Code: profile.value.connectionCode, IdToken: 'string' };
    try {
        await axios.post(apiUrl, payload);
        // No es necesario toast aquí, ya se muestra al generar.
        // userStore.$reset(); // Considera si esto es necesario aquí o interfiere.
        // updateProfileFromStore();
    } catch (error) {
        const errorMsg = error.response?.data?.Message || 'Error al registrar el código de conexión.';
        toast.add({ severity: 'error', summary: 'Error de Registro de Código', detail: errorMsg, life: 3000 });
    }
}
async function registerConnectionCodeAdmin() {
    const userId = userStore.fullUserData?.userId;
    if (!profile.value.connectionCode || !userId) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'No se ha generado código o falta ID de usuario.', life: 3000 });
        return;
    }
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/registerCodeConnection';
    const payload = { CurrentUserId: `${userId};admin:True`, Code: profile.value.connectionCode, IdToken: 'string' };
    try {
        const response = await axios.post(apiUrl, payload);
        toast.add({ severity: 'success', summary: 'Éxito', detail: response.data?.Message || 'Código admin registrado.', life: 3000 });
        // userStore.$reset(); // Considera si esto es necesario aquí.
        // updateProfileFromStore();
    } catch (error) {
        const errorMsg = error.response?.data?.Message || 'Error al registrar el código de conexión admin.';
        toast.add({ severity: 'error', summary: 'Error de Registro de Código Admin', detail: errorMsg, life: 3000 });
    }
}

async function deleteConnectionCode(codeToDelete) {
    const code = codeToDelete || profile.value.connectionCodeInput;
    if (!code) {
        // No mostrar error si es llamado internamente y no hay código (ej. después de conectar)
        // toast.add({ severity: 'warn', summary: 'Aviso', detail: 'No hay código de conexión para eliminar.', life: 3000 });
        return;
    }
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/deleteCodeConnection';
    const payload = { Code: code, IdToken: 'string' };
    try {
        await axios.post(apiUrl, payload);
        if (code === profile.value.connectionCodeInput) {
            // Solo limpiar si era el código ingresado
            profile.value.connectionCodeInput = null;
        }
        // toast.add({ severity: 'info', summary: 'Código Eliminado', detail: 'El código de conexión ha sido eliminado.', life: 3000 });
    } catch (error) {
        const errorMsg = error.response?.data?.Message || 'Error al eliminar el código de conexión.';
        toast.add({ severity: 'error', summary: 'Error de Eliminación de Código', detail: errorMsg, life: 3000 });
    }
}

async function deleteAllData() {
    isMoreOptionsDialogOpen.value = false;
    confirm.require({
        message: '¿Estás seguro de que deseas eliminar todos tus datos de salud? Esta acción no se puede deshacer.',
        header: 'Confirmar Eliminación Total de Datos',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, Eliminar Todo',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const userId = userStore.fullUserData?.userId;
            if (!userId) {
                toast.add({ severity: 'error', summary: 'Error', detail: 'ID de usuario no disponible.', life: 3000 });
                return;
            }
            const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/deletedata';
            const payload = { UserId: userId, Token: 'string' };
            try {
                const response = await axios.delete(apiUrl, { data: payload });
                if (response.status === 200 && (response.data?.success || response.data?.Message)) {
                    // Adaptar a la respuesta del backend
                    toast.add({ severity: 'success', summary: 'Éxito', detail: response.data?.Message || 'Todos tus datos han sido eliminados.', life: 3000 });
                    // Considera si es necesario resetear el store o recargar datos.
                    // userStore.$reset();
                    // updateProfileFromStore(); // O redirigir
                } else {
                    toast.add({ severity: 'error', summary: 'Error', detail: response.data?.ErrorMessage || response.data?.Message || 'No se pudieron eliminar los datos.', life: 3000 });
                }
            } catch (error) {
                const errorMsg = error.response?.data?.Message || error.message || 'Error al procesar la solicitud de eliminación.';
                toast.add({ severity: 'error', summary: 'Error de Eliminación', detail: errorMsg, life: 3000 });
            }
        },
        reject: () => {
            toast.add({ severity: 'info', summary: 'Cancelado', detail: 'La eliminación de datos ha sido cancelada.', life: 3000 });
        }
    });
}

async function downloadPdf() {
    // URL del endpoint, ajústala según la configuración de tu API
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/ReadData/get-pdf';
    const payload = {
        Credentials: {
            UserId: userStore.fullUserData.userId,
            IdToken: 'string'
        }
    };

    try {
        const response = await axios.post(apiUrl, payload, { responseType: 'blob' });
        const blob = new Blob([response.data], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', 'datosSalud.pdf');
        document.body.appendChild(link);
        link.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(link);
    } catch (error) {
        toast.add({ severity: 'error', summary: 'Error', detail: error.message });
        console.error('Error al descargar el PDF:', error);
    }
}

// --- Nueva función para descargar PDF por tipo de dato ---
async function downloadPdfBySelectedType(dataTypeValue, dataTypeLabel, fileNamePart) {
    const userId = userStore.fullUserData?.userId;
    if (!userId) {
        toast.add({ severity: 'error', summary: 'Error', detail: 'ID de usuario no disponible.', life: 3000 });
        return;
    }

    // Endpoint del C# Artifact para PDF por tipo
    const apiUrl = 'https://bluttruck.grial.eu/backend/api/ReadData/get-pdf-by-type';
    const payload = {
        Credentials: {
            UserId: userId,
            IdToken: 'string'
        },
        DataType: dataTypeValue // El valor numérico del enum
    };

    try {
        toast.add({ severity: 'info', summary: 'Procesando', detail: `Generando PDF para ${dataTypeLabel}...`, life: 2000 });
        const response = await axios.post(apiUrl, payload, { responseType: 'blob' });

        // Verificar si la respuesta es realmente un PDF o un error JSON en blob
        if (response.data.type === 'application/json') {
            const errorJson = await response.data.text();
            const errorObj = JSON.parse(errorJson);
            toast.add({ severity: 'error', summary: 'Error del Servidor', detail: errorObj.Message || `No se pudo generar el PDF para ${dataTypeLabel}.`, life: 5000 });
            console.error(`Error del servidor al generar PDF para ${dataTypeLabel}:`, errorObj);
            return;
        }

        const blob = new Blob([response.data], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `datosSalud_${userId}_${fileNamePart}.pdf`);
        document.body.appendChild(link);
        link.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(link);
        toast.add({ severity: 'success', summary: 'Éxito', detail: `PDF para ${dataTypeLabel} descargado.`, life: 3000 });
    } catch (error) {
        // Si el error ya fue manejado como un JSON blob, no mostrar otro toast.
        if (error.response && error.response.data && error.response.data.type === 'application/json') {
            // Ya se manejó arriba
        } else {
            const errorMsg = error.response?.data?.Message || (error.response?.data instanceof Blob ? `Error al procesar el archivo PDF para ${dataTypeLabel}.` : error.message) || `Error desconocido al descargar PDF para ${dataTypeLabel}.`;
            toast.add({ severity: 'error', summary: 'Error de Descarga', detail: errorMsg, life: 5000 });
        }
        console.error(`Error al descargar PDF para ${dataTypeLabel}:`, error.response || error);
    } finally {
        isDataTypeSelectDialogOpen.value = false; // Cierra el diálogo de selección
    }
}

async function deleteUserAccount() {
    isMoreOptionsDialogOpen.value = false;
    confirm.require({
        message: '¿Estás ABSOLUTAMENTE SEGURO de que deseas eliminar tu cuenta? Esta acción es irreversible y borrará todos tus datos y tu usuario de forma permanente.',
        header: 'Confirmación MUY IMPORTANTE de Eliminación de Cuenta',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Sí, Entiendo y Quiero Eliminar Mi Cuenta',
        rejectLabel: 'Cancelar',
        acceptClass: 'p-button-danger',
        accept: async () => {
            const userId = userStore.fullUserData?.userId;
            if (!userId) {
                toast.add({ severity: 'error', summary: 'Error', detail: 'ID de usuario no disponible para eliminar la cuenta.', life: 3000 });
                return;
            }
            const apiUrl = 'https://bluttruck.grial.eu/backend/api/WriteData/deleteuser';
            const payload = { UserId: userId, Token: 'string' }; // El backend espera 'Token'
            try {
                const response = await axios.delete(apiUrl, { data: payload });
                if (response.status === 200 && (response.data?.success || response.data?.Message)) {
                    // Adaptar a la respuesta del backend
                    toast.add({ severity: 'success', summary: 'Cuenta Eliminada', detail: response.data?.Message || 'Tu cuenta ha sido eliminada permanentemente.', life: 5000 });
                    userStore.$reset();
                    router.push('/'); // Redirigir a la página de inicio
                } else {
                    toast.add({ severity: 'error', summary: 'Error', detail: response.data?.Message || 'No se pudo eliminar la cuenta.', life: 3000 });
                }
            } catch (error) {
                const errorMsg = error.response?.data?.Message || error.message || 'Error al procesar la solicitud de eliminación de cuenta.';
                toast.add({ severity: 'error', summary: 'Error de Eliminación de Cuenta', detail: errorMsg, life: 5000 });
            }
        },
        reject: () => {
            toast.add({ severity: 'info', summary: 'Cancelado', detail: 'La eliminación de la cuenta ha sido cancelada.', life: 3000 });
        }
    });
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
                <div class="mb-4 flex items-center">
                    <input
                        type="checkbox"
                        id="smoke"
                        v-model="profile.smoke"
                        class="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:focus:ring-blue-600 dark:ring-offset-surface-800"
                    />
                    <label for="smoke" class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Fumador</label>
                </div>
                <div class="mb-4 flex items-center">
                    <input
                        type="checkbox"
                        id="alcohol"
                        v-model="profile.alcohol"
                        class="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:focus:ring-blue-600 dark:ring-offset-surface-800"
                    />
                    <label for="alcohol" class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Bebe Alcohol</label>
                </div>
                <div class="mb-4 flex items-center">
                    <input
                        type="checkbox"
                        id="predisposition"
                        v-model="profile.hasPredisposition"
                        class="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:focus:ring-blue-600 dark:ring-offset-surface-800"
                    />
                    <label for="predisposition" class="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Predisposición Genética</label>
                </div>
                <div class="mb-4 w-full">
                    <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nivel de Colesterol (1-5)</label>
                    <Slider v-model="profile.cholesterol" :min="0" :max="5" :step="1" class="w-full my-slider" />
                    <div class="text-xs text-gray-600 dark:text-gray-400 mt-1 text-center">{{ profile.cholesterol > 0 ? profile.cholesterol * 40 + ' mg/dL' : 'No especificado' }}</div>
                </div>
            </div>

            <div class="flex flex-col">
                <div class="mb-4">
                    <label for="name" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Nombre</label>
                    <input type="text" id="name" v-model="profile.name" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label for="weight" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Peso (kg)</label>
                    <input type="number" id="weight" v-model="profile.weight" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label for="height" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Altura (cm)</label>
                    <input type="number" id="height" v-model="profile.height" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                </div>
                <div class="mb-4">
                    <label for="dob" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Fecha de Nacimiento</label>
                    <Calendar
                        inputId="dob"
                        v-model="profile.dateOfBirth"
                        dateFormat="yy-mm-dd"
                        showIcon
                        :showButtonBar="true"
                        class="w-full [&>input]:p-2 [&>input]:w-full [&>input]:border [&>input]:rounded-md [&>input]:shadow-sm dark:[&>input]:bg-surface-700 dark:[&>input]:border-surface-600 dark:[&>input]:text-white"
                    />
                </div>
                <div class="mb-4">
                    <label for="gender" class="block text-sm font-medium text-gray-700 dark:text-gray-300">Género</label>
                    <select id="gender" v-model="profile.gender" class="mt-1 p-2 w-full border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white">
                        <option disabled value="">Seleccione género</option>
                        <option value="Hombre">Hombre</option>
                        <option value="Mujer">Mujer</option>
                        <option value="Otro">Otro</option>
                    </select>
                </div>
                <div class="mb-4">
                    <button
                        @click="saveProfile"
                        class="w-full bg-blue-500 hover:bg-blue-600 text-white p-3 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-offset-surface-800"
                    >
                        Guardar Cambios
                    </button>
                </div>
            </div>

            <div class="col-span-1 md:col-span-2 flex flex-col items-center mt-6 border-t pt-6 dark:border-surface-700">
                <h3 class="text-xl font-semibold mb-4 text-gray-800 dark:text-gray-200">Generar Código de Conexión</h3>
                <div class="flex justify-center space-x-4 mb-4">
                    <button @click="() => generateConnectionCodeAdmin('admin')" class="bg-green-500 hover:bg-green-600 text-white py-2 px-4 rounded-md shadow-sm">Generar Código Admin</button>
                    <button @click="() => generateConnectionCode('normal')" class="bg-sky-500 hover:bg-sky-600 text-white py-2 px-4 rounded-md shadow-sm">Generar Código Normal</button>
                </div>
                <div v-if="profile.connectionCode" class="mt-2 p-3 border rounded-md bg-gray-100 dark:bg-surface-700 text-center">
                    <span class="text-sm text-gray-700 dark:text-gray-300">Código ({{ profile.connectionType }}): </span>
                    <strong class="text-lg text-gray-900 dark:text-white">{{ profile.connectionCode }}</strong>
                </div>
            </div>

            <div class="col-span-1 md:col-span-2 flex flex-col items-center mt-6 border-t pt-6 dark:border-surface-700">
                <h3 class="text-xl font-semibold mb-4 text-gray-800 dark:text-gray-200">Conectar con Otra Persona</h3>
                <input
                    type="text"
                    v-model="profile.connectionCodeInput"
                    placeholder="Introduce el código de conexión"
                    class="mt-1 p-2 w-full max-w-xs border rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white"
                />
                <button @click="connectWithCode" class="w-full max-w-xs bg-indigo-500 hover:bg-indigo-600 text-white p-3 rounded-md shadow-sm mt-2">Conectar</button>
            </div>

            <div class="col-span-1 md:col-span-2 flex justify-center mt-8 space-x-4 border-t pt-6 dark:border-surface-700">
                <button @click="signOut" class="bg-orange-500 hover:bg-orange-600 text-white py-2 px-4 rounded-md shadow-sm">Cerrar Sesión</button>
                <button @click="openMoreOptionsDialog" class="bg-gray-500 hover:bg-gray-600 text-white py-2 px-4 rounded-md shadow-sm">Más Opciones</button>
            </div>
        </div>

        <Dialog v-model:visible="isMoreOptionsDialogOpen" modal header="Más Opciones" :style="{ width: '90vw', maxWidth: '400px' }" :breakpoints="{ '960px': '75vw', '640px': '90vw' }" dismissableMask>
            <div class="flex flex-col space-y-3 p-1">
                <p class="text-gray-700 dark:text-gray-300 mb-2">Selecciona una acción:</p>
                <button @click="downloadPdf" class="w-full bg-purple-500 hover:bg-purple-600 text-white p-3 rounded-md shadow-sm">Obtener Todos los Datos (PDF)</button>
                <button @click="openDataTypeSelectDialog" class="w-full bg-teal-500 hover:bg-teal-600 text-white p-3 rounded-md shadow-sm">Imprimir Dato a Elegir (PDF)</button>
                <button @click="deleteAllData" class="w-full bg-red-500 hover:bg-red-600 text-white p-3 rounded-md shadow-sm">Eliminar Todos los Datos</button>
                <button @click="deleteUserAccount" class="w-full bg-red-700 hover:bg-red-800 text-white p-3 rounded-md shadow-sm">Eliminar Cuenta (Usuario)</button>
            </div>
        </Dialog>

        <Dialog v-model:visible="isDataTypeSelectDialogOpen" modal header="Seleccionar Dato para PDF" :style="{ width: '90vw', maxWidth: '500px' }" :breakpoints="{ '960px': '75vw', '640px': '90vw' }" dismissableMask>
            <div class="p-1">
                <p class="text-gray-700 dark:text-gray-300 mb-4">Elige el tipo de dato que deseas incluir en el informe PDF:</p>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <button
                        v-for="option in healthDataTypeOptions"
                        :key="option.value"
                        @click="downloadPdfBySelectedType(option.value, option.label, option.fileNamePart)"
                        class="w-full bg-blue-500 hover:bg-blue-600 text-white p-3 rounded-md shadow-sm text-sm text-left"
                    >
                        {{ option.label }}
                    </button>
                </div>
            </div>
        </Dialog>
    </div>
</template>

<style scoped>
/* Estilos para PrimeVue Slider (si es necesario personalizarlos más allá de las clases de Tailwind) */
:deep(.my-slider .p-slider-range) {
    background-color: #3b82f6 !important; /* Tailwind blue-500 */
}
:deep(.my-slider .p-slider-handle) {
    border-color: #3b82f6 !important; /* Tailwind blue-500 */
    background-color: #ffffff !important; /* Fondo blanco para el handle */
}
:deep(.my-slider .p-slider-handle:focus) {
    box-shadow: 0 0 0 0.2rem rgba(59, 130, 246, 0.5) !important; /* Sombra de foco azul */
}

/* Estilos para PrimeVue Calendar para que ocupe el ancho completo y inputs tengan padding */
/* Se usan clases de Tailwind directamente en el componente Calendar para mayor especificidad */

/* Estilos generales para inputs y selects para consistencia */
input[type='text'],
input[type='number'],
select,
.p-calendar input {
    /* Tailwind ya aplica estilos, pero puedes añadir más aquí si es necesario */
    /* Ejemplo: dark:placeholder-gray-500 */
}
</style>
