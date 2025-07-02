<script setup>
import { ref } from 'vue';

const profile = ref({
    name: 'Carlos',
    weight: 70,
    height: 175,
    dateOfBirth: '1990-01-01',
    gender: 'Hombre',
    smoke: false,
    alcohol: false,
    hasPredisposition: false,
    imageUrl: '/demo/images/monitoreo/hombre1.jpg'
});
</script>

<template>
    <div class="bg-white dark:bg-surface-800 p-8 rounded-lg shadow-lg w-full max-w-4xl grid grid-cols-1 md:grid-cols-2 gap-8">
        <div class="flex flex-col items-center md:items-start">
            <h2 class="text-3xl font-bold mb-6 text-center md:text-left text-gray-800 dark:text-gray-200">Bienvenido, {{ profile.name }}</h2>
            <div class="mb-6">
                <img :src="profile.imageUrl" alt="Foto de perfil" class="w-32 h-32 rounded-full object-cover" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Fumador</label>
                <input type="checkbox" v-model="profile.smoke" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Bebe Alcohol</label>
                <input type="checkbox" v-model="profile.alcohol" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Predisposición</label>
                <input type="checkbox" v-model="profile.hasPredisposition" />
            </div>
        </div>

        <div class="flex flex-col">
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Nombre</label>
                <input type="text" v-model="profile.name" class="input-field" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Peso (kg)</label>
                <input type="number" v-model="profile.weight" class="input-field" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Altura (cm)</label>
                <input type="number" v-model="profile.height" class="input-field" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Fecha de Nacimiento</label>
                <input type="date" v-model="profile.dateOfBirth" class="input-field" />
            </div>
            <div class="mb-4">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">Género</label>
                <select v-model="profile.gender" class="input-field">
                    <option value="Hombre">Hombre</option>
                    <option value="Mujer">Mujer</option>
                    <option value="Otro">Otro</option>
                </select>
            </div>
        </div>
    </div>
</template>

<style scoped>
.input-field {
    width: 100%;
    padding: 8px;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    outline: none;
    transition: border-color 0.2s;
}

.input-field:focus {
    border-color: #6366f1;
}
</style>
