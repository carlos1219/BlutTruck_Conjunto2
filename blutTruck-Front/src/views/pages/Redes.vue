<script>
import Monitoreo from '@/components/Monitoreo.vue';
import Perfil from '@/components/MonitoreoPerfil.vue';
import axios from 'axios';
import Dialog from 'primevue/dialog';
import ProgressSpinner from 'primevue/progressspinner';
import { useUserStore } from '/stores/user';

// 1. Importar componentes y hooks de PrimeVue
import ConfirmDialog from 'primevue/confirmdialog';
import Toast from 'primevue/toast';
import { useConfirm } from 'primevue/useconfirm';
import { useToast } from 'primevue/usetoast';

const DEFAULT_AVATAR_URL = '/demo/images/monitoreo/hombre1.png';
const API_BASE_URL_READ = 'https://bluttruck.grial.eu/backend/api/ReadData';
const API_BASE_URL_WRITE = 'https://bluttruck.grial.eu/backend/api/WriteData';

export default {
    name: 'MonitorScreen',
    components: {
        Dialog,
        Monitoreo,
        Perfil,
        ProgressSpinner,
        ConfirmDialog, // 2. Registrar ConfirmDialog
        Toast // 2. Registrar Toast
    },
    // 3. Añadir setup para usar los hooks
    setup() {
        const confirmService = useConfirm();
        const toastService = useToast();
        // Devolver para poder usarlos con 'this' en methods
        return { confirmService, toastService };
    },
    data() {
        return {
            dialogVisible: false,
            adminDialogVisible: false,
            selectedPerson: {},
            personasQueMeMonitorean: [],
            personasQueMonitoreo: [],
            isLoading: false,
            error: null,
            isFetchingFullData: false,
            fullDataError: null,
            userStore: useUserStore(),
            showingPerfilView: false
        };
    },
    methods: {
        async fetchMonitoringData() {
            this.isLoading = true;
            this.error = null;
            const userId = localStorage.getItem('userId');
            let idToken = localStorage.getItem('idToken') || 'string';

            if (!userId) {
                this.error = 'No se pudo obtener la identificación del usuario.';
                this.isLoading = false;
                console.error(this.error);
                // Usar Toast para errores iniciales también podría ser una opción
                this.toastService.add({ severity: 'error', summary: 'Error de Autenticación', detail: this.error, life: 5000 });
                return;
            }

            const credentialsPayload = { Credentials: { UserId: userId, IdToken: idToken } };

            try {
                const [connectedUsersResponse, monitoringUsersResponse] = await Promise.all([axios.post(`${API_BASE_URL_READ}/connected-users`, credentialsPayload), axios.post(`${API_BASE_URL_WRITE}/getMonitoringUsers`, credentialsPayload)]);

                this.personasQueMonitoreo =
                    connectedUsersResponse.data?.map((user) => ({
                        id: user.connectedUserId,
                        name: user.name,
                        img: user.photoURL && user.photoURL !== 'No photo' ? user.photoURL : DEFAULT_AVATAR_URL
                    })) ?? [];
                if (!connectedUsersResponse?.data) {
                    console.warn("No se recibieron datos válidos de /connected-users para 'personasQueMonitoreo'");
                }

                this.personasQueMeMonitorean =
                    monitoringUsersResponse.data?.map((user) => ({
                        id: user.monitoringUserId,
                        name: user.name,
                        img: user.photoURL && user.photoURL !== 'No photo' ? user.photoURL : DEFAULT_AVATAR_URL,
                        email: user.email,
                        administrador: user.isAdmin
                    })) ?? [];
                if (!monitoringUsersResponse?.data) {
                    console.warn("No se recibieron datos válidos de /getMonitoringUsers para 'personasQueMeMonitorean'");
                }
            } catch (err) {
                console.error('Error al obtener los datos de monitoreo:', err);
                const errorDetail = err.response?.data?.Message || err.message || 'Error desconocido.';
                this.error = `Error al cargar listas: ${errorDetail}`;
                this.toastService.add({ severity: 'error', summary: 'Error al Cargar Listas', detail: errorDetail, life: 5000 });
                this.personasQueMeMonitorean = [];
                this.personasQueMonitoreo = [];
            } finally {
                this.isLoading = false;
            }
        },

        async openDialog(person) {
            console.log('Abriendo diálogo para la persona monitoreada:', person.name, 'ID:', person.id);
            this.selectedPerson = person;
            this.fullDataError = null;
            this.isFetchingFullData = true;
            this.showingPerfilView = false; // Asegúrate de resetear la vista
            this.dialogVisible = true;

            // --- Obtener los datos necesarios ---
            const currentUserUserId = localStorage.getItem('userId'); // ID del usuario actual (el que está usando la app - el monitor)
            const idToken = localStorage.getItem('idToken') || 'string'; // Token del usuario actual
            const monitoredUserId = person.id; // ID del usuario seleccionado (el monitoreado)

            // --- Validar que tenemos el ID del usuario actual ---
            if (!currentUserUserId) {
                console.error('Error crítico: No se encontró el UserId del usuario actual en localStorage.');
                this.fullDataError = 'Error de autenticación. No se pudo identificar al usuario.';
                this.toastService.add({ severity: 'error', summary: 'Error de Autenticación', detail: this.fullDataError, life: 5000 });
                this.isFetchingFullData = false;
                return; // Detener la ejecución
            }

            // --- Construir el payload con la estructura del DTO C# ---
            const payload = {
                Credentials: {
                    UserId: monitoredUserId, // ID del usuario MONITOR (el que hace la petición)
                    IdToken: idToken // Token del usuario MONITOR
                },
                UserMonitoringID: currentUserUserId // ID del usuario MONITOREADO (aquel cuyos datos se quieren ver)
            };

            // Opcional: Log para verificar la estructura antes de enviar
            console.log('Enviando payload a /fullAndConnection:', JSON.stringify(payload));

            try {
                // Asegúrate que la URL es la correcta - usas /fullAndConnection en tu código
                const response = await axios.post(`${API_BASE_URL_READ}/fullAndConnection`, payload); // <--- Payload modificado

                if (response && response.data) {
                    // Asumiendo que la respuesta ahora incluye CurrentUserIsAdmin
                    // Ejemplo: response.data = { DatosPersonales: {...}, Dias: {...}, CurrentUserIsAdmin: true }
                    const monitoredDataWithId = {
                        ...response.data,
                        monitoredUserId: person.id // Esto es útil solo para el frontend si lo necesitas
                    };
                    this.userStore.setMonitoredUserData(monitoredDataWithId);
                    console.log('Datos completos guardados:', monitoredDataWithId); // Verifica que CurrentUserIsAdmin llegó
                } else {
                    console.warn('La respuesta de /fullAndConnection no contiene datos válidos para:', person.name);
                    this.fullDataError = 'No se recibieron datos detallados.';
                    this.userStore.setMonitoredUserData(null);
                    this.toastService.add({ severity: 'warn', summary: 'Sin Datos', detail: `No se encontraron datos detallados para ${person.name}.`, life: 3000 });
                }
            } catch (err) {
                console.error(`Error al obtener datos completos para ${person.name}:`, err);
                const errorDetail = err.response?.data?.Message || err.response?.data || err.message || 'Error desconocido.';
                const errorStatus = err.response?.status;

                this.fullDataError = `Error al cargar detalles (${errorStatus || 'sin código'}): ${errorDetail}`;
                this.userStore.setMonitoredUserData(null); // Limpiar datos en caso de error

                // Manejo específico de errores comunes
                if (errorStatus === 403) {
                    this.fullDataError = `No tienes permiso para ver los detalles de ${person.name}.`;
                    this.toastService.add({ severity: 'error', summary: 'Acceso Denegado', detail: this.fullDataError, life: 5000 });
                } else if (errorStatus === 401) {
                    this.fullDataError = `Error de autenticación. Por favor, inicia sesión de nuevo.`;
                    this.toastService.add({ severity: 'error', summary: 'Autenticación Fallida', detail: this.fullDataError, life: 5000 });
                } else {
                    this.toastService.add({ severity: 'error', summary: 'Error al Cargar Detalles', detail: this.fullDataError, life: 5000 });
                }
            } finally {
                this.isFetchingFullData = false;
            }
        }, // Fin del método openDialog

        toggleAdmin(persona) {
            const currentStateIsAdmin = String(persona.administrador).toLowerCase() === 'true';
            const nuevoValor = currentStateIsAdmin ? 'False' : 'True';
            const userId = localStorage.getItem('userId');
            let idToken = localStorage.getItem('idToken') || 'string';
            const payload = {
                Credentials: { UserId: userId, IdToken: idToken },
                AdminId: nuevoValor,
                UserExtractId: persona.id
            };

            axios
                .post(`${API_BASE_URL_WRITE}/writeAdmin`, payload)
                .then((response) => {
                    console.log(`Estado de admin para ${persona.name} actualizado a ${nuevoValor}`, response.data);
                    persona.administrador = nuevoValor; // Actualizar UI tras éxito
                    this.toastService.add({ severity: 'success', summary: 'Permisos Actualizados', detail: `Permisos para ${persona.name} actualizados.`, life: 3000 });
                })
                .catch((err) => {
                    console.error(`Error al actualizar estado de admin para ${persona.name}`, err);
                    const errorDetail = err.response?.data?.Message || err.message || 'Error desconocido.';
                    this.toastService.add({ severity: 'error', summary: 'Error al Actualizar Permisos', detail: errorDetail, life: 5000 });
                    // No revertimos aquí para evitar parpadeo si el usuario intenta de nuevo
                });
        },

        // 5. Modificar confirmDelete para usar ConfirmDialog y Toast
        confirmDelete(person, listType) {
            // Obtener referencias a los servicios de ConfirmDialog y Toast
            const confirm = this.confirmService;
            const toast = this.toastService;

            // --- Lógica para REVOCAR ACCESO (Personas que ME Monitorean) ---
            if (listType === 'monitoringMe') {
                const userToRevokeName = person.name;
                const userToRevokeId = person.id; // ID del usuario al que se le revoca el acceso

                console.log('Acción: Iniciar revocación de acceso para:', userToRevokeName, '(ID:', userToRevokeId, ')');

                // Mostrar diálogo de confirmación de PrimeVue
                confirm.require({
                    message: `¿Seguro que quieres revocar el acceso de ${userToRevokeName} a tus datos? Esta acción eliminará la conexión de forma permanente.`,
                    header: 'Confirmar Revocación',
                    icon: 'pi pi-exclamation-triangle', // Icono de advertencia
                    acceptLabel: 'Sí, Revocar', // Texto botón aceptar
                    rejectLabel: 'Cancelar', // Texto botón rechazar
                    accept: () => {
                        // Función a ejecutar si se acepta
                        const currentUserId = localStorage.getItem('userId'); // ID del usuario actual (el que revoca)
                        let idToken = localStorage.getItem('idToken') || 'string';

                        // Validar IDs
                        if (!currentUserId) {
                            toast.add({ severity: 'error', summary: 'Error Crítico', detail: 'No se pudo obtener tu ID. Por favor, inicia sesión de nuevo.', life: 5000 });
                            console.error('Current user ID not found in localStorage.');
                            return;
                        }
                        if (!userToRevokeId) {
                            toast.add({ severity: 'error', summary: 'Error Crítico', detail: `No se pudo obtener el ID del usuario ${userToRevokeName}.`, life: 5000 });
                            console.error('User ID to revoke not found in person object:', person);
                            return;
                        }

                        // Construir payload (IDs normales para este caso)
                        const payload = {
                            Credentials: { UserId: userToRevokeId, IdToken: idToken }, // ID del revocado
                            ConnectedUserId: currentUserId // ID del que revoca (tú)
                        };
                        const apiUrl = `${API_BASE_URL_WRITE}/deleteConnection`;

                        // Llamada API
                        axios
                            .delete(apiUrl, { data: payload })
                            .then((response) => {
                                console.log(`Conexión eliminada con éxito para ${userToRevokeName}`, response.data);
                                toast.add({ severity: 'success', summary: 'Acceso Revocado', detail: `Se ha revocado el acceso correctamente para ${userToRevokeName}.`, life: 3000 });
                                // Actualizar UI
                                this.personasQueMeMonitorean = this.personasQueMeMonitorean.filter((p) => p.id !== userToRevokeId);
                            })
                            .catch((err) => {
                                console.error(`Error al intentar revocar el acceso para ${userToRevokeName}:`, err.response || err.message || err);
                                const errorMsg = err.response?.data?.Message || err.message || 'Ocurrió un error desconocido.';
                                toast.add({ severity: 'error', summary: 'Error al Revocar', detail: `No se pudo revocar el acceso: ${errorMsg}`, life: 5000 });
                            });
                    },
                    reject: () => {
                        // Función a ejecutar si se rechaza
                        toast.add({ severity: 'info', summary: 'Cancelado', detail: 'Revocación de acceso cancelada.', life: 3000 });
                    }
                });
            }
            // --- Lógica para DEJAR DE MONITOREAR (Personas que YO Monitoreo) ---
            else if (listType === 'iMonitor') {
                const userToUnfollowName = person.name;
                const userToUnfollowId = person.id; // ID del usuario al que se deja de seguir

                console.log('Acción: Dejar de monitorear a:', userToUnfollowName, '(ID:', userToUnfollowId, ')');

                // Mostrar diálogo de confirmación de PrimeVue
                confirm.require({
                    message: `¿Seguro que quieres dejar de monitorear a ${userToUnfollowName}?`,
                    header: 'Confirmar Acción',
                    icon: 'pi pi-info-circle', // Icono informativo
                    acceptLabel: 'Sí, Dejar de Monitorear', // Texto botón aceptar
                    rejectLabel: 'Cancelar', // Texto botón rechazar
                    accept: () => {
                        // Función a ejecutar si se acepta
                        const currentUserId = localStorage.getItem('userId'); // ID del usuario actual (el que deja de seguir)
                        let idToken = localStorage.getItem('idToken') || 'string';

                        // Validar IDs
                        if (!currentUserId) {
                            toast.add({ severity: 'error', summary: 'Error Crítico', detail: 'No se pudo obtener tu ID. Por favor, inicia sesión de nuevo.', life: 5000 });
                            return;
                        }
                        if (!userToUnfollowId) {
                            toast.add({ severity: 'error', summary: 'Error Crítico', detail: `No se pudo obtener el ID del usuario ${userToUnfollowName}.`, life: 5000 });
                            return;
                        }

                        // Construir payload (IDs invertidos para este caso, como pediste)
                        const payload = {
                            Credentials: { UserId: currentUserId, IdToken: idToken }, // ID del que deja de seguir (tú)
                            ConnectedUserId: userToUnfollowId // ID del que es dejado de seguir
                        };
                        const apiUrl = `${API_BASE_URL_WRITE}/deleteConnection`;

                        // Llamada API
                        axios
                            .delete(apiUrl, { data: payload })
                            .then((response) => {
                                console.log(`Dejaste de monitorear a ${userToUnfollowName}`, response.data);
                                toast.add({ severity: 'success', summary: 'Monitoreo Detenido', detail: `Has dejado de monitorear a ${userToUnfollowName}.`, life: 3000 });
                                // Actualizar UI
                                this.personasQueMonitoreo = this.personasQueMonitoreo.filter((p) => p.id !== userToUnfollowId);
                            })
                            .catch((err) => {
                                console.error(`Error al dejar de monitorear a ${userToUnfollowName}:`, err.response || err.message || err);
                                const errorMsg = err.response?.data?.Message || err.message || 'Ocurrió un error desconocido.';
                                toast.add({ severity: 'error', summary: 'Error', detail: `No se pudo dejar de monitorear: ${errorMsg}`, life: 5000 });
                            });
                    },
                    reject: () => {
                        // Función a ejecutar si se rechaza
                        toast.add({ severity: 'info', summary: 'Cancelado', detail: 'Operación cancelada.', life: 3000 });
                    }
                });
            }
        }, // Fin de confirmDelete

        perfilStats() {
            console.log('Mostrando vista de Perfil para:', this.selectedPerson.name);
            this.showingPerfilView = true;
        },

        switchToMonitoreoView() {
            this.showingPerfilView = false;
        },

        closeStatsDialog() {
            this.dialogVisible = false;
            this.userStore.setMonitoredUserData(null);
            this.selectedPerson = {};
            this.fullDataError = null;
            this.showingPerfilView = false;
        },

        openAdminDialogForMonitor(person) {
            console.log('Abriendo diálogo admin para quien ME monitorea:', person.name);
            this.selectedPerson = person;
            this.adminDialogVisible = true;
        }
    },
    async mounted() {
        await this.fetchMonitoringData();
    }
};
</script>

<template>
    <div class="min-h-screen flex flex-col dark:bg-surface-900 p-4 md:p-6">
        <Toast position="top-right" />
        <ConfirmDialog />

        <div v-if="isLoading" class="text-center p-4 dark:text-gray-300"><ProgressSpinner style="width: 50px; height: 50px" strokeWidth="8" /> Cargando listas...</div>
        <div v-if="error && !isLoading" class="text-center p-4 text-red-500 bg-red-100 dark:bg-red-900 dark:text-red-200 border border-red-400 dark:border-red-700 rounded mb-4"><strong>Error:</strong> {{ error }}</div>

        <div v-if="!isLoading && !error" class="flex flex-col lg:flex-row space-y-6 lg:space-y-0 lg:space-x-6 justify-center items-start">
            <div class="flex-1 bg-white p-4 rounded-lg shadow dark:bg-surface-800 w-full max-w-2xl mx-auto">
                <h2 class="text-xl font-bold mb-4 text-center dark:text-white">Personas que me Monitorean</h2>
                <div v-if="!personasQueMeMonitorean.length" class="text-center text-gray-500 dark:text-gray-400 py-4">Nadie te está monitoreando actualmente.</div>
                <div v-else class="space-y-3">
                    <div v-for="persona in personasQueMeMonitorean" :key="'monitoring-me-' + persona.id" class="bg-gray-50 w-full dark:bg-surface-700 rounded-lg p-3 flex flex-col sm:flex-row items-center gap-3 sm:gap-4">
                        <div class="flex items-center flex-grow min-w-0 w-full sm:w-auto">
                            <img :src="persona.img" :alt="'Foto de ' + persona.name" class="w-12 h-12 sm:w-14 sm:h-14 rounded-full object-cover bg-gray-300 dark:bg-gray-600 flex-shrink-0" />
                            <span class="ml-3 sm:ml-4 self-center text-base sm:text-lg dark:text-white truncate" :title="persona.name">
                                {{ persona.name }}
                            </span>
                        </div>

                        <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-center sm:justify-end gap-2 w-full sm:w-auto mt-2 sm:mt-0 flex-shrink-0">
                            <span
                                @click="toggleAdmin(persona)"
                                :class="[
                                    'cursor-pointer px-3 py-1.5 rounded flex items-center justify-center text-white text-sm whitespace-nowrap',
                                    String(persona.administrador).toLowerCase() === 'true' ? 'bg-green-500 hover:bg-green-600' : 'bg-gray-500 hover:bg-gray-600'
                                ]"
                                title="Permitir/Denegar que vea datos detallados"
                            >
                                <span :class="[String(persona.administrador).toLowerCase() === 'true' ? 'pi pi-check' : 'pi pi-ban', 'mr-2']"></span>
                                <span>{{ String(persona.administrador).toLowerCase() === 'true' ? 'Permitido' : 'Denegado' }}</span>
                            </span>
                            <button @click.stop="confirmDelete(persona, 'monitoringMe')" class="bg-red-500 hover:bg-red-600 text-white px-3 py-1.5 rounded flex items-center justify-center text-sm whitespace-nowrap">
                                <span class="pi pi-trash mr-2"></span>
                                <span>Revocar Acceso</span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <div class="flex-1 bg-white p-4 rounded-lg shadow dark:bg-surface-800 w-full max-w-2xl mx-auto">
                <h2 class="text-xl font-bold mb-4 text-center dark:text-white">Personas que Monitoreo</h2>
                <div v-if="!personasQueMonitoreo.length" class="text-center text-gray-500 dark:text-gray-400 py-4">No estás monitoreando a nadie actualmente.</div>
                <div v-else class="space-y-3">
                    <div
                        v-for="persona in personasQueMonitoreo"
                        :key="'i-monitor-' + persona.id"
                        class="bg-gray-50 w-full dark:bg-surface-700 rounded-lg p-3 flex flex-col sm:flex-row items-center gap-3 sm:gap-4 cursor-pointer hover:bg-gray-100 dark:hover:bg-surface-600 transition duration-150 ease-in-out"
                        @click="openDialog(persona)"
                    >
                        <div class="flex items-center flex-grow min-w-0 w-full sm:w-auto">
                            <img :src="persona.img" :alt="'Foto de ' + persona.name" class="w-12 h-12 sm:w-14 sm:h-14 rounded-full object-cover bg-gray-300 dark:bg-gray-600 flex-shrink-0" />
                            <span class="ml-3 sm:ml-4 self-center text-base sm:text-lg dark:text-white truncate" :title="persona.name">
                                {{ persona.name }}
                            </span>
                        </div>

                        <div class="flex flex-col sm:flex-row items-stretch sm:items-center justify-center sm:justify-end gap-2 w-full sm:w-auto mt-2 sm:mt-0 flex-shrink-0">
                            <button @click.stop="confirmDelete(persona, 'iMonitor')" class="bg-orange-500 hover:bg-orange-600 text-white px-3 py-1.5 rounded flex items-center justify-center text-sm whitespace-nowrap">
                                <span class="pi pi-user-minus mr-2"></span>
                                <span>Dejar de Monitorear</span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <Dialog
            v-model:visible="dialogVisible"
            :header="showingPerfilView ? 'Perfil de ' + (selectedPerson.name || 'Usuario') : 'Detalles de ' + (selectedPerson.name || 'Usuario Monitoreado')"
            modal
            dismissableMask
            class="w-11/12 md:w-5/6 lg:w-3/4 xl:w-2/3 bg-white dark:bg-surface-800"
            @hide="closeStatsDialog"
        >
            <div class="p-4 min-h-[400px]">
                <!-- Contenido principal del diálogo -->
                <div v-if="isFetchingFullData" class="flex flex-col justify-center items-center h-full">
                    <ProgressSpinner style="width: 60px; height: 60px" strokeWidth="6" />
                    <p class="mt-3 dark:text-gray-300">Cargando datos detallados para {{ selectedPerson.name }}...</p>
                </div>
                <div v-else-if="fullDataError" class="text-center p-4 text-red-600 dark:text-red-300">
                    <p><strong>Error al cargar los datos:</strong></p>
                    <p>{{ fullDataError }}</p>
                </div>
                <div v-else>
                    <div v-if="!showingPerfilView">
                        <div v-if="userStore.monitoredUserData">
                            <Monitoreo :user-data="userStore.monitoredUserData" />
                        </div>
                        <div v-else class="text-center text-gray-500 dark:text-gray-400">No hay datos detallados disponibles para mostrar.</div>
                    </div>
                    <div v-else>
                        <Perfil />
                    </div>
                </div>
            </div>

            <template #footer v-if="userStore.monitoredUserData && userStore.monitoredUserData.currentUserIsAdmin && userStore.monitoredUserData.currentUserIsAdmin.toLowerCase() === 'true'">
                <div class="w-full flex flex-wrap justify-between items-center gap-2 px-4 py-3 bg-gray-50 dark:bg-surface-700 rounded-b-lg">
                    <button v-if="!showingPerfilView" @click="perfilStats" class="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded flex items-center whitespace-nowrap">
                        <span class="pi pi-user-edit mr-2"></span>
                        <span>Perfil</span>
                    </button>
                    <button v-else @click="switchToMonitoreoView" class="bg-gray-500 hover:bg-gray-600 text-white px-4 py-2 rounded flex items-center whitespace-nowrap">
                        <span class="pi pi-arrow-left mr-2"></span>
                        <span>Volver a Monitoreo</span>
                    </button>
                    <button @click="closeStatsDialog" class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded flex items-center ml-auto whitespace-nowrap">
                        <span class="pi pi-times mr-2"></span>
                        <span>Cerrar</span>
                    </button>
                </div>
            </template>
        </Dialog>
    </div>
</template>

<style scoped>
img {
    object-fit: cover;
}

.dark .bg-gray-50 {
    background-color: var(--surface-700);
}
.dark .hover\:bg-gray-100:hover {
    background-color: var(--surface-600);
}
.dark .text-gray-500 {
    color: var(--text-color-secondary);
}

/* Opcional: Estilos para el ConfirmDialog si necesitas personalización */
/* :deep(.p-confirm-dialog .p-dialog-content) { ... } */
</style>
