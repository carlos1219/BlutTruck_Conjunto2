import { createApp } from 'vue';
import App from './App.vue';
import router from './router';

import Aura from '@primevue/themes/aura';
import 'primeicons/primeicons.css';
import PrimeVue from 'primevue/config';
import ConfirmationService from 'primevue/confirmationservice';
import ToastService from 'primevue/toastservice';
import Tooltip from 'primevue/tooltip'; // Importa Tooltip

import '@/assets/styles.scss';
import '@/assets/tailwind.css';

// Importa y crea la instancia de Pinia
import { createPinia } from 'pinia';
const pinia = createPinia();

const app = createApp(App);

app.use(router);
app.use(pinia); // Agrega Pinia
app.use(PrimeVue, {
    theme: {
        preset: Aura,
        options: {
            darkModeSelector: '.app-dark'
        }
    }
});
app.use(ToastService);
app.use(ConfirmationService);

app.directive('tooltip', Tooltip); // Registra la directiva tooltip
app.mount('#app');
