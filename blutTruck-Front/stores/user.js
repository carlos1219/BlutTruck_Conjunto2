// stores/user.js
import { defineStore } from 'pinia';

export const useUserStore = defineStore('user', {
    state: () => ({
        fullUserData: null,
        dias: {},
        monitoredUserData: null
    }),
    // NUEVO: Getters para acceder de forma segura a los datos.
    getters: {
        /**
         * Devuelve el array de favoritos del usuario.
         * Si no existen, devuelve un array vacío para evitar errores en el componente.
         * @returns {string[]}
         */
        favoriteCards: (state) => state.fullUserData?.favorites || []
    },
    actions: {
        // NUEVA ACCIÓN: Un 'setter' simple para los favoritos.
        /**
         * Actualiza el estado local de los favoritos.
         * Esta acción será llamada desde el componente DESPUÉS de que la API haya respondido con éxito.
         * @param {string[]} favoritesArray - El nuevo array de favoritos.
         */
        setFavorites(favoritesArray) {
            if (this.fullUserData) {
                this.fullUserData.favorites = favoritesArray;
            }
        },
        setUserData(data) {
            this.fullUserData = data;
        },
        setDias(diasData) {
            this.dias = diasData;
        },
        setMonitoredUserData(data) {
            this.monitoredUserData = data;
        },
        updateDiasInUserData(newDias) {
            if (this.fullUserData) {
                // Actualiza solo la propiedad "dias" en fullUserData
                this.fullUserData = { ...this.fullUserData, dias: newDias };
            } else {
                // Si fullUserData aún no existe, puedes inicializarla con la propiedad "dias"
                this.fullUserData = { dias: newDias };
            }
        }
    }
});
