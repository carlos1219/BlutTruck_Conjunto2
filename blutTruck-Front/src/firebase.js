// src/firebase.js
import { getAnalytics } from 'firebase/analytics';
import { initializeApp } from 'firebase/app';
import { getAuth } from 'firebase/auth';

const firebaseConfig = {
    apiKey: 'AIzaSyB03cPKHoZJ05WaIT_D-Vsmsy7bkzH4zIc',
    authDomain: 'proyectocsharp-tfg.firebaseapp.com',
    databaseURL: 'https://proyectocsharp-tfg-default-rtdb.europe-west1.firebasedatabase.app',
    projectId: 'proyectocsharp-tfg',
    storageBucket: 'proyectocsharp-tfg.firebasestorage.app',
    messagingSenderId: '1092918958210',
    appId: '1:1092918958210:web:7f49e990983582dfd7fc9b',
    measurementId: 'G-55GHTPC0Z5'
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app); // Analytics está bien si lo necesitas

const auth = getAuth(app);

export { auth };
