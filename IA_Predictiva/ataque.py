from flask import Flask, request, jsonify
import pandas as pd
import joblib
import numpy as np

# Cargar el modelo y el scaler previamente entrenados
modelo = joblib.load("svm_model.pkl")
scaler = joblib.load("scaler.pkl")  # Guardar el scaler usado en el entrenamiento

# Crear la app Flask
app = Flask(__name__)

@app.route('/predecir', methods=['POST'])
def predecir():
    try:
        # Obtener los datos enviados por el cliente
        datos = request.get_json()
        print("➡️ Datos recibidos:", datos)

        if not datos:
            return jsonify({"error": "No se enviaron datos"}), 400

        # Convertir los datos a un DataFrame
        datos_df = pd.DataFrame([datos])
        print("📊 DataFrame generado:\n", datos_df)

        # Aplicar el preprocesamiento (scaling)
        datos_escalados = scaler.transform(datos_df)
        print("📐 Datos escalados:\n", datos_escalados)

        # Realizar la predicción
        prediccion = modelo.predict(datos_escalados)
        print("🎯 Predicción:", prediccion)

        # Responder con la predicción
        return jsonify({
            "prediccion": prediccion[0]
        })

    except Exception as e:
        print("💥 Error:", str(e))
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
