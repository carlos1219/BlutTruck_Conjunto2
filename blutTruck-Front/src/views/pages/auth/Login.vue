<script setup>
import axios from 'axios';
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useUserStore } from '/stores/user.js';

// --- Importa Firebase Auth y el proveedor de Google ---
import { GoogleAuthProvider, signInWithPopup } from 'firebase/auth';
// Asegúrate que la ruta a tu archivo firebase.js sea correcta
import { auth } from '/src/firebase'; // <-- Verifica esta ruta

const email = ref('');
const password = ref('');
const checked = ref(false);
const errorMessage = ref('');
const router = useRouter();
const userStore = useUserStore();
const baseUrl = 'https://bluttruck.grial.eu/backend';
const isForgotPasswordDialogVisible = ref(false);
const resetEmail = ref('');
const resetMessage = ref('');
const isResetError = ref(false);
const isSendingResetLink = ref(false);

// Lógica para recordar email/pass (sin cambios)
const storedEmail = localStorage.getItem('rememberMeEmail');
const storedPassword = localStorage.getItem('rememberMePassword');
if (storedEmail && storedPassword) {
    email.value = storedEmail;
    password.value = storedPassword;
    checked.value = true;
}

// --- Función LOGIN CON EMAIL/PASSWORD ---
// Llama a tu backend que usa Firebase por debajo
const loginUser = async () => {
    if (!email.value || !password.value) {
        errorMessage.value = 'Correo y contraseña son obligatorios.';
        return;
    }
    errorMessage.value = '';
    try {
        // 1. Llama a tu backend /login
        const loginResponse = await axios.post(`${baseUrl}/api/WriteData/login`, {
            Email: email.value,
            Password: password.value
        });

        // 2. Extrae las credenciales de Firebase devueltas por TU backend
        //    (Asumiendo que devuelve { userId: firebaseUid, token: firebaseIdToken, ... })
        const backendResponseData = loginResponse.data;
        const firebaseUid = backendResponseData.userId; // Este es el Firebase UID
        const firebaseIdToken = backendResponseData.token; // Este es el Firebase ID Token

        if (!firebaseUid || !firebaseIdToken) {
            throw new Error('La respuesta del login no contiene userId o token.');
        }

        // 3. Guarda las credenciales de Firebase en localStorage
        localStorage.setItem('userId', firebaseUid); // Guarda Firebase UID como 'userId'
        localStorage.setItem('token', firebaseIdToken); // Guarda Firebase ID Token como 'token'

        // 4. Gestiona 'Recuérdame' (sin cambios)
        if (checked.value) {
            localStorage.setItem('rememberMeEmail', email.value);
            localStorage.setItem('rememberMePassword', password.value);
        } else {
            localStorage.removeItem('rememberMeEmail');
            localStorage.removeItem('rememberMePassword');
        }

        const fullDataResponse = await axios.post(
            `${baseUrl}/api/ReadData/full`,
            {
                UserId: firebaseUid, // Envía Firebase UID
                IdToken: 'string' // Envía Firebase ID Token
            },
            {
                headers: { Authorization: `Bearer ${firebaseIdToken}` } // Envía Firebase ID Token
            }
        );

        // 6. Procesa respuesta, actualiza store y redirige
        localStorage.setItem('fullUserData', JSON.stringify(fullDataResponse.data));
        userStore.setUserData({ userId: firebaseUid, ...fullDataResponse.data });
        router.push('/dashboard');
    } catch (error) {
        console.error('Error en loginUser:', error);
        if (error.response) {
            errorMessage.value = error.response.data?.Message || 'Error del servidor al iniciar sesión.';
        } else if (error.message.includes('userId o token')) {
            errorMessage.value = 'Error procesando la respuesta del login.';
        } else {
            errorMessage.value = 'Error al iniciar sesión con email/contraseña.';
        }
        // Limpia tokens/datos si falla
        localStorage.removeItem('token');
        localStorage.removeItem('userId');
        localStorage.removeItem('fullUserData');
    }
};

// --- Función LOGIN CON GOOGLE usando FIREBASE ---
const signInWithGoogleFirebase = async () => {
    const provider = new GoogleAuthProvider();
    errorMessage.value = ''; // Limpia errores

    try {
        // 1. Abre el Popup de Google Sign-In gestionado por Firebase JS SDK
        const result = await signInWithPopup(auth, provider);
        const user = result.user; // Usuario de Firebase

        // 2. Obtén las credenciales de Firebase directamente
        const firebaseUid = user.uid; // Firebase UID
        const firebaseIdToken = await user.getIdToken(); // Firebase ID Token

        console.log('Login con Google exitoso. Firebase UID:', firebaseUid);

        // 3. Guarda las credenciales de Firebase en localStorage
        localStorage.setItem('userId', firebaseUid); // Guarda Firebase UID como 'userId'
        localStorage.setItem('token', firebaseIdToken); // Guarda Firebase ID Token como 'token'

        // 4. Limpia otros datos de sesión si existieran
        localStorage.removeItem('rememberMeEmail');
        localStorage.removeItem('rememberMePassword');

        // 5. *** LLAMA A /api/ReadData/full USANDO LAS CREDENCIALES DE FIREBASE ***
        //     (Exactamente igual que en la función loginUser)
        console.log(`Llamando a /api/ReadData/full para userId: ${firebaseUid}`);
        const fullDataResponse = await axios.post(
            `${baseUrl}/api/ReadData/full`,
            {
                UserId: firebaseUid, // Envía Firebase UID
                IdToken: 'string' // Envía Firebase ID Token
            },
            {
                headers: { Authorization: `Bearer ${firebaseIdToken}` } // Envía Firebase ID Token
            }
        );

        // 6. Procesa respuesta, actualiza store y redirige
        localStorage.setItem('fullUserData', JSON.stringify(fullDataResponse.data));
        userStore.setUserData({ userId: firebaseUid, ...fullDataResponse.data });
        router.push('/dashboard');
    } catch (error) {
        console.error('Error en Google Sign-In:', error);
        if (error.response) {
            // Error de Axios (probablemente de ReadData/full)
            errorMessage.value = `Error del servidor: ${error.response.data?.Message || 'Operación fallida'}`;
        } else if (error.code) {
            // Error específico de Firebase (ej: popup cerrado)
            errorMessage.value = `Error de Firebase: ${error.code}`;
        } else {
            // Otro error
            errorMessage.value = 'Error al iniciar sesión con Google.';
        }
        // Limpia tokens/datos si falla
        localStorage.removeItem('token');
        localStorage.removeItem('userId');
        localStorage.removeItem('fullUserData');
    }
};
const goToLandingPage = () => {
    router.push('/'); // Navega a la ruta raíz
};

// Función para abrir el diálogo y limpiar su estado
const openForgotPasswordDialog = () => {
    resetEmail.value = '';
    resetMessage.value = '';
    isResetError.value = false;
    isForgotPasswordDialogVisible.value = true;
};

// ¡ESTA ES LA LÓGICA CLAVE! Llama a tu backend, no a Firebase
const handlePasswordReset = async () => {
    if (!resetEmail.value) {
        isResetError.value = true;
        resetMessage.value = 'Por favor, introduce tu correo electrónico.';
        return;
    }

    isSendingResetLink.value = true;
    isResetError.value = false;
    resetMessage.value = '';

    try {
        // Llama al endpoint que creaste en tu AuthController
        const response = await axios.post(`${baseUrl}/api/WriteData/forgot-password`, {
            Email: resetEmail.value
        });

        // Muestra el mensaje de éxito que te devuelve el backend
        resetMessage.value = response.data.message;
        isResetError.value = false;
    } catch (error) {
        console.error('Error al llamar al backend para reseteo:', error);
        isResetError.value = true;
        // Muestra un mensaje genérico en caso de error de red u otro problema.
        resetMessage.value = 'Ocurrió un error en el servidor. Inténtalo de nuevo más tarde.';
    } finally {
        isSendingResetLink.value = false;
    }
};
</script>

<template>
    <FloatingConfigurator />
    <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden relative p-4 sm:p-8">
        <Button
            icon="pi pi-arrow-left"
            class="!absolute top-4 left-4 sm:top-8 sm:left-8 p-button-rounded p-button-text !text-surface-700 !dark:text-surface-200 !hover:bg-surface-100 !dark:hover:bg-surface-800"
            @click="goToLandingPage"
            aria-label="Volver a la página de inicio"
        />

        <div class="flex flex-col items-center justify-center">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, #3498db 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 !border-blue-500" style="border-radius: 53px">
                    <div class="text-center mb-8">
                        <img src="/demo/images/logo.png" alt="Logo BlutTruck" class="h-24 mx-auto" />
                        <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">¡Bienvenido a BlutTruck!</div>
                        <span class="text-muted-color font-medium">Inicia sesión para continuar</span>
                    </div>

                    <div>
                        <label for="email1" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2"> Correo electrónico </label>
                        <InputText id="email1" type="text" placeholder="Correo electrónico" class="w-full md:w-[30rem] mb-8 focus:!border-blue-500" v-model="email" />

                        <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2"> Contraseña </label>
                        <InputText id="password1" type="password" v-model="password" placeholder="Contraseña" class="mb-4 focus:!border-blue-500 w-full" @keyup.enter="loginUser" />

                        <div class="flex items-center justify-between mt-2 mb-8 gap-8">
                            <div class="flex items-center">
                                <input type="checkbox" id="rememberme1" v-model="checked" class="mr-2 border rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 dark:bg-surface-700 dark:border-surface-600 dark:text-white" />
                                <label for="rememberme1">Recuérdame</label>
                            </div>

                            <a @click="openForgotPasswordDialog" class="font-medium no-underline ml-2 text-right cursor-pointer text-blue-500 hover:text-blue-700"> ¿Has olvidado la contraseña? </a>
                        </div>

                        <Button label="Iniciar sesión" class="w-full !bg-blue-500 !border-blue-500 mb-4" @click="loginUser"></Button>

                        <div class="text-center my-4 text-muted-color font-medium">O</div>

                        <Button label="Iniciar sesión con Google" icon="pi pi-google" class="!border-blue-500 !text-blue-500 w-full p-button-outlined mb-4" @click="signInWithGoogleFirebase"></Button>

                        <div v-if="errorMessage" class="mt-4 text-red-500 text-center">{{ errorMessage }}</div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <Dialog header="Restablecer Contraseña" v-model:visible="isForgotPasswordDialogVisible" modal :style="{ width: '25rem' }">
        <span class="text-muted-color block mb-4"> Ingresa tu correo electrónico y te enviaremos un enlace para restablecer tu contraseña. </span>
        <div class="flex flex-col gap-2">
            <label for="reset-email" class="font-semibold">Correo electrónico</label>
            <InputText id="reset-email" class="w-full" v-model="resetEmail" @keyup.enter="handlePasswordReset" />
        </div>

        <div v-if="resetMessage" class="mt-4 text-center p-2 rounded-md" :class="isResetError ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'">
            {{ resetMessage }}
        </div>

        <template #footer>
            <Button label="Cancelar" class="!text-blue-500" @click="isForgotPasswordDialogVisible = false" text />
            <Button label="Enviar Enlace" class="!bg-blue-500 !border-blue-500" @click="handlePasswordReset" :loading="isSendingResetLink" />
        </template>
    </Dialog>
</template>

<style scoped>
/* Estilos existentes (sin cambios) */
.pi-eye,
.pi-eye-slash {
    transform: scale(1.6);
    margin-right: 1rem;
}
.pi-google {
    margin-right: 0.5rem;
}

.pi-arrow-left {
    font-size: 2.5rem;
}
</style>
