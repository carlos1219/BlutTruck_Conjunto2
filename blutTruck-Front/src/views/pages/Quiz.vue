<script>
export default {
    data() {
        return {
            allQuestions: [
                {
                    text: '¿Cuál es un factor de riesgo común para los ataques cardíacos?',
                    options: ['Comer vegetales', 'Fumar', 'Hacer ejercicio regularmente', 'Beber mucha agua'],
                    correctAnswer: 'Fumar'
                },
                {
                    text: '¿Qué es un síntoma común de un ataque cardíaco?',
                    options: ['Dolor de cabeza', 'Dolor en el pecho', 'Dolor de estómago', 'Dolor de espalda'],
                    correctAnswer: 'Dolor en el pecho'
                },
                {
                    text: '¿Qué tipo de grasa es más perjudicial para la salud del corazón?',
                    options: ['Grasa saturada', 'Grasa insaturada', 'Grasa trans', 'Ácidos grasos omega-3'],
                    correctAnswer: 'Grasa trans'
                },
                {
                    text: '¿Cuál de los siguientes hábitos puede reducir el riesgo de un ataque cardíaco?',
                    options: ['Fumar', 'Consumir alcohol en exceso', 'Hacer ejercicio regularmente', 'Comer alimentos fritos'],
                    correctAnswer: 'Hacer ejercicio regularmente'
                },
                {
                    text: '¿Qué es la presión arterial alta?',
                    options: ['Un signo de buena salud', 'Un factor de riesgo para ataques cardíacos', 'Una condición sin importancia', 'Un signo de deshidratación'],
                    correctAnswer: 'Un factor de riesgo para ataques cardíacos'
                },
                {
                    text: '¿Qué es el colesterol LDL?',
                    options: ['Colesterol bueno', 'Colesterol malo', 'Colesterol neutro', 'Colesterol dietético'],
                    correctAnswer: 'Colesterol malo'
                },
                {
                    text: '¿Cuál de los siguientes alimentos es bueno para la salud del corazón?',
                    options: ['Papas fritas', 'Aguacate', 'Hamburguesa', 'Pizza'],
                    correctAnswer: 'Aguacate'
                },
                {
                    text: '¿Qué es la angina de pecho?',
                    options: ['Un tipo de dolor de cabeza', 'Un dolor en el pecho causado por falta de oxígeno', 'Un dolor de estómago', 'Un dolor en la espalda'],
                    correctAnswer: 'Un dolor en el pecho causado por falta de oxígeno'
                },
                {
                    text: '¿Cuál de los siguientes es un signo de un ataque cardíaco en las mujeres?',
                    options: ['Dolor de cabeza', 'Fatiga inusual', 'Dolor de estómago', 'Dolor en la espalda'],
                    correctAnswer: 'Fatiga inusual'
                },
                {
                    text: '¿Qué es un electrocardiograma (ECG)?',
                    options: ['Una prueba para medir el azúcar en sangre', 'Una prueba para medir la actividad eléctrica del corazón', 'Una prueba para medir la presión arterial', 'Una prueba para medir el colesterol'],
                    correctAnswer: 'Una prueba para medir la actividad eléctrica del corazón'
                },
                {
                    text: '¿Cuál de los siguientes es un medicamento común para tratar la presión arterial alta?',
                    options: ['Aspirina', 'Paracetamol', 'Losartán', 'Ibuprofeno'],
                    correctAnswer: 'Losartán'
                },
                {
                    text: '¿Qué es la aterosclerosis?',
                    options: ['Un tipo de infección', 'El endurecimiento de las arterias', 'Un tipo de alergia', 'Un tipo de dolor de cabeza'],
                    correctAnswer: 'El endurecimiento de las arterias'
                },
                {
                    text: '¿Cuál de los siguientes es un síntoma de insuficiencia cardíaca?',
                    options: ['Dolor de cabeza', 'Hinchazón en los pies', 'Dolor de estómago', 'Dolor de espalda'],
                    correctAnswer: 'Hinchazón en los pies'
                },
                {
                    text: '¿Qué es un marcapasos?',
                    options: ['Un dispositivo para medir la presión arterial', 'Un dispositivo para regular el ritmo cardíaco', 'Un dispositivo para medir el azúcar en sangre', 'Un dispositivo para medir el colesterol'],
                    correctAnswer: 'Un dispositivo para regular el ritmo cardíaco'
                },
                {
                    text: '¿Cuál de los siguientes es un factor de riesgo para la enfermedad cardíaca?',
                    options: ['Bajo peso', 'Obesidad', 'Dieta vegetariana', 'Ejercicio diario'],
                    correctAnswer: 'Obesidad'
                },
                {
                    text: '¿Qué es un infarto de miocardio?',
                    options: ['Un tipo de dolor de cabeza', 'Un ataque cardíaco', 'Un tipo de infección', 'Un tipo de alergia'],
                    correctAnswer: 'Un ataque cardíaco'
                },
                {
                    text: '¿Cuál de los siguientes es un síntoma de un ataque cardíaco silencioso?',
                    options: ['Dolor agudo en el pecho', 'Sudoración', 'Náuseas', 'Dolor de cabeza'],
                    correctAnswer: 'Náuseas'
                },
                {
                    text: '¿Qué es la cardiomiopatía?',
                    options: ['Una enfermedad del hígado', 'Una enfermedad del corazón', 'Una enfermedad del riñón', 'Una enfermedad del pulmón'],
                    correctAnswer: 'Una enfermedad del corazón'
                },
                {
                    text: '¿Cuál de los siguientes es un tratamiento común para la angina?',
                    options: ['Antibióticos', 'Nitroglicerina', 'Antihistamínicos', 'Analgésicos'],
                    correctAnswer: 'Nitroglicerina'
                },
                {
                    text: '¿Qué es un stent coronario?',
                    options: ['Un tipo de medicamento', 'Un dispositivo para mantener abiertas las arterias', 'Un tipo de dieta', 'Un tipo de ejercicio'],
                    correctAnswer: 'Un dispositivo para mantener abiertas las arterias'
                },
                {
                    text: '¿Cuál de los siguientes es un signo de un ataque cardíaco?',
                    options: ['Dolor de cabeza', 'Dificultad para respirar', 'Dolor de estómago', 'Dolor de espalda'],
                    correctAnswer: 'Dificultad para respirar'
                },
                {
                    text: '¿Qué es la fibrilación auricular?',
                    options: ['Un tipo de ritmo cardíaco irregular', 'Un tipo de infección', 'Un tipo de alergia', 'Un tipo de dolor de cabeza'],
                    correctAnswer: 'Un tipo de ritmo cardíaco irregular'
                },
                {
                    text: '¿Cuál de los siguientes es un factor de riesgo para la enfermedad cardíaca en mujeres?',
                    options: ['Menopausia', 'Embarazo', 'Lactancia', 'Uso de anticonceptivos'],
                    correctAnswer: 'Menopausia'
                },
                {
                    text: '¿Qué es un bypass coronario?',
                    options: ['Un tipo de dieta', 'Una cirugía para desviar el flujo sanguíneo', 'Un tipo de ejercicio', 'Un tipo de medicamento'],
                    correctAnswer: 'Una cirugía para desviar el flujo sanguíneo'
                },
                {
                    text: '¿Cuál de los siguientes es un síntoma de un ataque cardíaco en hombres?',
                    options: ['Dolor en el brazo izquierdo', 'Dolor de cabeza', 'Dolor de estómago', 'Dolor de espalda'],
                    correctAnswer: 'Dolor en el brazo izquierdo'
                },
                {
                    text: '¿Qué es la insuficiencia cardíaca congestiva?',
                    options: ['Una enfermedad del hígado', 'Una enfermedad del corazón', 'Una enfermedad del riñón', 'Una enfermedad del pulmón'],
                    correctAnswer: 'Una enfermedad del corazón'
                },
                {
                    text: '¿Cuál de los siguientes es un tratamiento común para la presión arterial alta?',
                    options: ['Dieta baja en sodio', 'Ejercicio regular', 'Medicamentos', 'Todos los anteriores'],
                    correctAnswer: 'Todos los anteriores'
                },
                {
                    text: '¿Qué es un ataque isquémico transitorio (AIT)?',
                    options: ['Un tipo de dolor de cabeza', 'Un mini derrame cerebral', 'Un tipo de infección', 'Un tipo de alergia'],
                    correctAnswer: 'Un mini derrame cerebral'
                },
                {
                    text: '¿Cuál de los siguientes es un síntoma de un ataque cardíaco?',
                    options: ['Mareo', 'Sudoración', 'Náuseas', 'Todos los anteriores'],
                    correctAnswer: 'Todos los anteriores'
                },
                {
                    text: '¿Qué es la enfermedad de las arterias coronarias?',
                    options: ['Una enfermedad del hígado', 'Una enfermedad del corazón', 'Una enfermedad del riñón', 'Una enfermedad del pulmón'],
                    correctAnswer: 'Una enfermedad del corazón'
                },
                {
                    text: '¿Cuál de los siguientes es un factor de riesgo para la enfermedad cardíaca?',
                    options: ['Diabetes', 'Hipertensión', 'Colesterol alto', 'Todos los anteriores'],
                    correctAnswer: 'Todos los anteriores'
                }
            ],
            questions: [],
            currentQuestionIndex: 0,
            selectedAnswer: null,
            score: 0,
            quizCompleted: false
        };
    },
    computed: {
        currentQuestion() {
            return this.questions[this.currentQuestionIndex];
        }
    },
    methods: {
        nextQuestion() {
            if (this.selectedAnswer === this.currentQuestion.correctAnswer) {
                this.score++;
            }
            this.currentQuestionIndex++;
            if (this.currentQuestionIndex >= this.questions.length) {
                this.quizCompleted = true;
            } else {
                this.selectedAnswer = null;
            }
        },
        previousQuestion() {
            if (this.currentQuestionIndex > 0) {
                this.currentQuestionIndex--;
                this.selectedAnswer = null;
            }
        },
        restartQuiz() {
            this.currentQuestionIndex = 0;
            this.selectedAnswer = null;
            this.score = 0;
            this.quizCompleted = false;
            this.selectRandomQuestions();
        },
        selectRandomQuestions() {
            const shuffled = this.allQuestions.sort(() => 0.5 - Math.random());
            this.questions = shuffled.slice(0, 10);
        },
        shuffleQuestions() {
            for (let i = this.questions.length - 1; i > 0; i--) {
                const j = Math.floor(Math.random() * (i + 1));
                [this.questions[i], this.questions[j]] = [this.questions[j], this.questions[i]];
            }
        }
    },
    created() {
        this.selectRandomQuestions();
    }
};
</script>

<template>
    <div class="outer-container">
        <div class="quiz-container">
            <h1 style="color: white; font-weight: bold">Quiz sobre Predicción de Ataques Cardíacos</h1>

            <div v-if="!quizCompleted">
                <div class="question">
                    <p>{{ currentQuestionIndex + 1 }}/10: {{ currentQuestion.text }}</p>
                    <ul class="options-grid">
                        <li v-for="(option, index) in currentQuestion.options" :key="index">
                            <label>
                                <input type="radio" :value="option" v-model="selectedAnswer" />
                                {{ option }}
                            </label>
                        </li>
                    </ul>
                </div>
                <div class="navigation-buttons">
                    <button @click="previousQuestion" :disabled="currentQuestionIndex === 0">Anterior</button>
                    <button @click="nextQuestion">Siguiente</button>
                </div>
            </div>
            <div v-else>
                <p>Quiz completado! Tu puntuación es: {{ score }}/10</p>
                <button @click="restartQuiz">Reiniciar Quiz</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
body,
html {
    height: 100%;
    margin: 0;
    display: flex;
    justify-content: center;
    align-items: center;
}

.quiz-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background-color: #3b82f6; /* Equivalente a bg-blue-500 */
    border-radius: 15px;
    padding: 20px;
    max-width: 600px;
    width: 100%;
    box-sizing: border-box;
    color: white;
}

.question {
    margin-bottom: 20px;
    width: 100%;
}

.options-grid {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
}

.options-grid li {
    flex: 1 1 calc(50% - 10px); /* Dos columnas con espacio entre ellas */
    margin-bottom: 10px;
}

.navigation-buttons {
    display: flex;
    justify-content: space-between;
    width: 100%;
}

button {
    padding: 10px 20px;
    font-size: 16px;
    background-color: white;
    color: #3b82f6;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    align-items: center;
}

button:hover {
    background-color: #e0e7ff;
}

button:disabled {
    background-color: #d1d5db;
    cursor: not-allowed;
}
.outer-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 70vh; /* Altura completa de la ventana */
}
</style>
