import AppLayout from '@/layout/AppLayout.vue';
import { createRouter, createWebHistory } from 'vue-router';
import { useUserStore } from '/stores/user'; //
const routes = [
    {
        path: '/dashboard', // Ruta padre para las vistas protegidas
        component: AppLayout,
        // PASO 1: Añadir meta a la ruta padre
        meta: { requiresAuth: true }, // <--- ¡AÑADIDO AQUÍ!
        // ------------------------------------
        children: [
            {
                path: '/dashboard', // Ruta exacta /dashboard
                name: 'dashboard',
                component: () => import('@/views/Dashboard.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/pages/crud',
                name: 'crud',
                component: () => import('@/views/pages/Crud.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/Redes',
                name: 'Redes',
                component: () => import('@/views/pages/Redes.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/Guardar_Datos',
                name: 'Guardar_Datos',
                component: () => import('@/views/pages/Guardar_Datos.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/Perfil',
                name: 'Perfil',
                component: () => import('@/views/pages/Perfil.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/Prediccion',
                name: 'Prediccion',
                component: () => import('@/views/pages/Prediccion.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/Quiz',
                name: 'Quiz',
                component: () => import('@/views/pages/Quiz.vue')
                // No necesita meta aquí, hereda del padre
            },
            {
                path: '/Aprende',
                name: 'Aprende',
                component: () => import('@/views/pages/Aprende.vue')
                // No necesita meta aquí, hereda del padre
            }
            // Todas las rutas hijas de '/dashboard' ahora requerirán autenticación
        ]
    },
    // --- Rutas Públicas ---
    {
        path: '/',
        name: 'landing',
        component: () => import('@/views/pages/Landing.vue')
        // No requiere autenticación
    },
    {
        path: '/pages/notfound',
        name: 'notfound',
        component: () => import('@/views/pages/NotFound.vue')
        // No requiere autenticación
    },
    {
        path: '/auth/login',
        name: 'login', // Nombre usado en la redirección del guardia
        component: () => import('@/views/pages/auth/Login.vue')
        // No requiere autenticación
    },
    {
        path: '/auth/register',
        name: 'register',
        component: () => import('@/views/pages/auth/Register.vue')
        // No requiere autenticación (normalmente)
    },
    {
        path: '/auth/access',
        name: 'accessDenied',
        component: () => import('@/views/pages/auth/Access.vue')
        // No requiere autenticación
    },
    {
        path: '/auth/error',
        name: 'error',
        component: () => import('@/views/pages/auth/Error.vue')
        // No requiere autenticación
    }
    // --- Captura cualquier ruta no definida (Opcional, si no tienes notfound global) ---
    // { path: '/:pathMatch(.*)*', name: 'NotFoundCatchAll', redirect: { name: 'notfound' } }
];

// Crear instancia del router
const router = createRouter({
    history: createWebHistory(), // Usando modo historia HTML5
    routes
});

// --- PASO 2: Añadir Guardia de Navegación Global ---
router.beforeEach((to, from, next) => {
    // PASO 3: Obtener la instancia del store *DENTRO* del guard
    const userStore = useUserStore();

    const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);

    // PASO 4: Comprobar autenticación usando el store de Pinia
    // Asumiendo que 'userId' en 'fullUserData' indica autenticación.
    // Usa optional chaining (?.) por si fullUserData no está inicializado aún.
    const isAuthenticated = !!userStore.fullUserData?.userId;

    // Alternativa (recomendada si la tienes): Usa un getter específico en tu store
    // const isAuthenticated = userStore.isLoggedIn; // Por ejemplo

    console.log(`Navegando a: ${to.path}, Requiere Auth: ${requiresAuth}, Autenticado (Pinia): ${isAuthenticated}`);

    if (requiresAuth && !isAuthenticated) {
        // SI requiere autenticación Y NO está autenticado (según Pinia) -> Redirigir a Login
        console.log('Acceso denegado (Pinia). Redirigiendo a /auth/login...');
        next({ name: 'login' });
    } else if ((to.name === 'login' || to.name === 'register') && isAuthenticated) {
        // SI YA está autenticado (según Pinia) e intenta ir a Login/Register -> Redirigir a dashboard
        console.log('Usuario ya autenticado (Pinia). Redirigiendo desde Login/Register a Dashboard...');
        next({ name: 'dashboard' });
    } else {
        // En cualquier otro caso -> Permitir navegación
        console.log('Acceso permitido (Pinia).');
        next();
    }
});
// -------------------------------------------------

export default router; // Exportar la instancia del router configurada
