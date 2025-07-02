<script setup>
// Tus imports existentes
import FloatingConfigurator from '@/components/FloatingConfigurator.vue';
import axios from 'axios'; // Cambiado fetch por axios para consistencia
import { ref } from 'vue';
import { useRouter } from 'vue-router';

// --- Importa Firebase Auth y el proveedor de Google ---
import { GoogleAuthProvider, signInWithPopup } from 'firebase/auth';
import { auth } from '/src/firebase'; // <-- Verifica esta ruta

const router = useRouter();

const name = ref('');
const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const checked = ref(false);
const message = ref(null); // Variable para mostrar notificaciones

// URL base de la API (Ajustada para que coincida con tu backend)
const baseUrl = 'https://bluttruck.grial.eu/backend'; // Asume que esta es tu base

// Expresiones regulares (sin cambios)
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;

// Función para registrar al usuario con EMAIL/PASSWORD (Usando Axios ahora)
const registerUser = async () => {
    // Validaciones (sin cambios)
    if (!name.value || !email.value || !password.value) {
        /*...*/ return;
    }
    if (!emailRegex.test(email.value)) {
        /*...*/ return;
    }
    if (!passwordRegex.test(password.value)) {
        /*...*/ return;
    }
    if (password.value !== confirmPassword.value) {
        /*...*/ return;
    }
    if (!checked.value) {
        /*...*/ return;
    }

    message.value = null; // Limpia mensajes previos

    const requestBody = {
        Name: name.value,
        Email: email.value,
        Password: password.value
    };

    try {
        // Llama a tu endpoint de registro existente
        const response = await axios.post(`${baseUrl}/api/WriteData/register`, requestBody);

        // Tu backend devuelve { Token: '...', Message: '...' } en caso de éxito
        message.value = { type: 'success', text: response.data.Message || 'Registro exitoso.' };
        // Opcional: guardar token y userId si el backend los devuelve aquí también
        // const token = response.data.Token;
        // const userId = ?; // Tu endpoint register no devuelve userId explícitamente
        // if (token) localStorage.setItem('token', token);
        // Redirige al login
        setTimeout(() => router.push('/auth/login'), 1500); // Pequeña pausa para leer el mensaje
    } catch (error) {
        console.error('Error en el registro (Email/Pass):', error);
        if (error.response && error.response.data) {
            message.value = { type: 'error', text: `Error: ${error.response.data.Message || 'No se pudo registrar.'}` };
        } else {
            message.value = { type: 'error', text: 'Ha ocurrido un error en el registro.' };
        }
    }
};

// --- NUEVA Función para REGISTRARSE/INICIAR SESIÓN con GOOGLE ---
const registerOrSignInWithGoogle = async () => {
    const provider = new GoogleAuthProvider();
    message.value = null; // Limpia mensajes

    try {
        // 1. Inicia sesión/registro con Google usando Firebase
        const result = await signInWithPopup(auth, provider);
        const user = result.user; // Usuario de Firebase
        const requestBody = {
            Name: user.displayName,
            Email: user.email,
            UserId: user.uid
        };

        await axios.post(`${baseUrl}/api/WriteData/googleRegister`, requestBody);
        router.push('/');

        // Alternativa: si no quieres auto-login, muestra mensaje y redirige a login
        // message.value = { type: 'success', text: 'Registro/Inicio de sesión con Google exitoso.' };
        // setTimeout(() => router.push('/auth/login'), 1500);
    } catch (error) {
        console.error('Error en Registro/Login con Google:', error);
        if (error.response) {
            // Error de Axios (probablemente del backend /complete o /full)
            message.value = { type: 'error', text: `Error del servidor: ${error.response.data?.message || error.response.data?.Message || 'Operación fallida'}` };
        } else if (error.code) {
            // Error específico de Firebase (ej: popup cerrado)
            message.value = { type: 'error', text: `Error de Firebase: ${error.code}` };
        } else {
            // Otro error
            message.value = { type: 'error', text: 'Error al procesar con Google.' };
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
</script>

<template>
    <FloatingConfigurator />
    <Button
        icon="pi pi-arrow-left"
        class="!absolute top-4 left-4 sm:top-8 sm:left-8 p-button-rounded p-button-text !text-surface-700 !dark:text-surface-200 !hover:bg-surface-100 !dark:hover:bg-surface-800"
        @click="goToLandingPage"
        aria-label="Volver a la página de inicio"
    />
    <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
        <div class="flex flex-col items-center justify-center">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, #3498db 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20 !border-blue-500" style="border-radius: 53px">
                    <div class="text-center mb-8">
                        <img src="/demo/images/logo.png" alt="Logo BlutTruck" class="h-24 mx-auto" />
                        <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">¡Regístrate en BlutTruck!</div>
                        <span class="text-muted-color font-medium">Crea una cuenta para continuar</span>
                    </div>

                    <div v-if="message" :class="{ 'text-green-600': message.type === 'success', 'text-red-600': message.type === 'error' }" class="mb-4 text-center whitespace-pre-line">{{ message.text }}</div>

                    <div>
                        <label for="name" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">Nombre</label>
                        <InputText id="name" type="text" placeholder="Nombre" class="w-full md:w-[30rem] mb-8 focus:!border-blue-500" v-model="name" />
                        <label for="email1" class="block text-surface-900 dark:text-surface-0 text-xl font-medium mb-2">Correo electrónico</label>
                        <InputText id="email1" type="text" placeholder="Correo electrónico" class="w-full md:w-[30rem] mb-8 focus:!border-blue-500" v-model="email" />
                        <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">Contraseña</label>
                        <InputText id="password1" type="password" v-model="password" placeholder="Contraseña" class="mb-4 focus:!border-blue-500 w-full" />
                        <label for="confirmPassword" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">Confirmar contraseña</label>
                        <InputText id="confirmPassword" type="password" v-model="confirmPassword" placeholder="Contraseña" class="mb-4 focus:!border-blue-500 w-full" />
                        <div class="flex items-center justify-between mt-2 mb-8 gap-8">
                            <div class="flex items-center">
                                <input v-model="checked" type="checkbox" id="terms" binary class="mr-2 focus:ring-blue-500 focus:border-blue-500" />
                                <label for="terms">
                                    Acepto los
                                    <a href="/Privacidad.pdf" target="_blank" class="text-blue-500 underline"> términos y condiciones </a>
                                </label>
                            </div>
                        </div>

                        <Button label="Registrarse" class="w-full !bg-blue-500 !border-blue-500 mb-4" @click="registerUser"></Button>

                        <div class="text-center my-4 text-muted-color font-medium">O</div>

                        <Button label="Continuar con Google" icon="pi pi-google" class="!border-blue-500 !text-blue-500 w-full p-button-outlined mb-4" @click="registerOrSignInWithGoogle"></Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
/* ... (tus estilos) ... */
.pi-google {
    margin-right: 0.5rem;
}
/* Para que los saltos de línea en mensajes de error funcionen */
.whitespace-pre-line {
    white-space: pre-line;
}
.pi-arrow-left {
    font-size: 2.5rem;
}
</style>
