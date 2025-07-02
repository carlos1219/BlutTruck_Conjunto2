<script setup>
import { ref } from 'vue';

import AppMenuItem from './AppMenuItem.vue';

const model = ref([
    {
        label: 'Inicio',
        items: [{ label: 'Mis Estadísticas', icon: 'pi pi-fw pi-home !text-blue-500', to: '/dashboard' }]
    },
    {
        label: 'Conexiones',
        items: [
            { label: 'Redes', icon: 'pi pi-fw pi-users !text-blue-500', to: '/Redes' },
            { label: 'Prediccion', icon: 'pi pi-fw pi-search !text-blue-500', to: '/Prediccion' }
        ]
    },
    {
        label: 'Datos',
        items: [
            { label: 'Guardar datos', icon: 'pi pi-fw pi-chart-line !text-blue-500', to: 'Guardar_Datos' },
            { label: 'Perfil', icon: 'pi pi-user !text-blue-500', to: 'Perfil' }
        ]
    },
    {
        label: 'Aprende',
        items: [
            { label: 'Quiz', icon: 'pi pi-fw pi-star !text-blue-500', to: '/Quiz' },
            { label: 'Guía ', icon: 'pi pi-fw pi-book !text-blue-500', to: '/Aprende' }
        ]
    }
]);
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="i">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
