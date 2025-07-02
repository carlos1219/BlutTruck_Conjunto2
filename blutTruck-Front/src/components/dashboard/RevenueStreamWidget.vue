<script setup>
import Calendar from 'primevue/calendar';
import Chart from 'primevue/chart';
import { computed, nextTick, onMounted, ref, watch } from 'vue'; // Importar nextTick
import { useUserStore } from '/stores/user'; // Asegúrate que la ruta es correcta

const userStore = useUserStore();
const chartSelectedDate = ref(null);
const chartComponentRef = ref(null);

const fullUserData = computed(() => userStore.fullUserData);

// --- Reactividad mejorada para datos y fechas ---
const data = computed(() => fullUserData.value);
const dias = computed(() => data.value?.dias ?? {});

const formatDate = (date) => {
    if (!date) return null;
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
};

const availableDates = computed(() => {
    return Object.keys(dias.value)
        .map((dateString) => {
            const parts = dateString.split('-');
            return new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2]));
        })
        .sort((a, b) => b.getTime() - a.getTime());
});

watch(
    availableDates,
    (newDates) => {
        if (newDates.length > 0) {
            const currentSelectedDateExists = chartSelectedDate.value && newDates.some((d) => d.getTime() === chartSelectedDate.value.getTime());
            if (!currentSelectedDateExists) {
                chartSelectedDate.value = newDates[0];
            }
        } else {
            chartSelectedDate.value = null;
        }
    },
    { immediate: true }
);

const isDateSelectable = (date) => {
    if (!date || !date.getFullYear) return false;
    return availableDates.value.some((availableDate) => availableDate.getFullYear() === date.getFullYear() && availableDate.getMonth() === date.getMonth() && availableDate.getDate() === date.getDate());
};

const dailyData = computed(() => {
    const formattedDate = formatDate(chartSelectedDate.value);
    return formattedDate && dias.value[formattedDate] ? dias.value[formattedDate] : {};
});

// --- Propiedades computadas para datos individuales (asegurando rawData) ---
const pressureData = computed(() => {
    const bpData = dailyData.value?.bloodPressureData ?? [];
    const labels = bpData.map((item) => new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    const systolicData = bpData.map((item) => item.systolic);
    const diastolicData = bpData.map((item) => item.diastolic);
    return { labels, systolicData, diastolicData, rawData: bpData };
});

const heartRateData = computed(() => {
    const hrData = dailyData.value?.heartRateData ?? [];
    const labels = hrData.map((item) => new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    const rateData = hrData.map((item) => item.bpm);
    return { labels, rateData, rawData: hrData };
});

const oxygenData = computed(() => {
    const oxData = dailyData.value?.oxygenSaturationData ?? [];
    const labels = oxData.map((item) => new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    const percentageData = oxData.map((item) => item.percentage);
    return { labels, percentageData, rawData: oxData };
});

const temperatureData = computed(() => {
    const tempData = dailyData.value?.temperatureData ?? [];
    const labels = tempData.map((item) => new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    const temperatureValues = tempData.map((item) => item.temperature);
    return { labels, temperatureValues, rawData: tempData };
});

const respiratoryRateSeriesData = computed(() => {
    const rrData = dailyData.value?.respiratoryRateData ?? [];
    if (!rrData || !Array.isArray(rrData)) return { labels: [], rateValues: [], rawData: [] };
    const labels = rrData.map((item) => new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    const rateValues = rrData.map((item) => item.rate);
    return { labels, rateValues, rawData: rrData };
});

const bloodGlucoseSeriesData = computed(() => {
    const bgData = dailyData.value?.bloodGlucoseData ?? [];
    if (!bgData || !Array.isArray(bgData)) return { labels: [], glucoseValues: [], rawData: [] };
    const labels = bgData.map((item) => new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    const glucoseValues = bgData.map((item) => item.bloodGlucose);
    return { labels, glucoseValues, rawData: bgData };
});

// --- Función auxiliar para procesar y alinear datos combinados ---
const processCombinedData = (dataSourcesConfig) => {
    const maps = dataSourcesConfig.map((config) => ({
        map: new Map(),
        rawData: config.sourceData?.rawData || [],
        valueExtractor: config.valueExtractor,
        key: config.key
    }));

    const allTimesSet = new Set();
    let hasValidRawData = false;

    maps.forEach((source) => {
        if (source.rawData && Array.isArray(source.rawData) && source.rawData.length > 0) {
            hasValidRawData = true;
            source.rawData.forEach((item) => {
                if (item && item.time) {
                    const timeKey = new Date(item.time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                    source.map.set(timeKey, source.valueExtractor(item));
                    allTimesSet.add(new Date(item.time).getTime());
                }
            });
        }
    });

    if (!hasValidRawData || allTimesSet.size === 0) {
        const emptyDatasets = {};
        maps.forEach((config) => {
            emptyDatasets[config.key] = [];
        });
        return { labels: [], ...emptyDatasets };
    }

    const sortedTimes = Array.from(allTimesSet)
        .map((time) => new Date(time))
        .sort((a, b) => a - b);
    const labels = sortedTimes.map((time) => time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));

    const resultDatasets = {};
    maps.forEach((config) => {
        resultDatasets[config.key] = labels.map((labelTime) => config.map.get(labelTime) ?? null);
    });

    return { labels, ...resultDatasets };
};

// --- Propiedades computadas para datos combinados ---
const combinedPressureHeartRateProcessedData = computed(() => {
    return processCombinedData([
        { sourceData: pressureData.value, valueExtractor: (item) => item.systolic, key: 'systolicData' },
        { sourceData: pressureData.value, valueExtractor: (item) => item.diastolic, key: 'diastolicData' },
        { sourceData: heartRateData.value, valueExtractor: (item) => item.bpm, key: 'heartRateData' }
    ]);
});

const combinedCardioRespO2ProcessedData = computed(() => {
    return processCombinedData([
        { sourceData: heartRateData.value, valueExtractor: (item) => item.bpm, key: 'heartRateValues' },
        { sourceData: respiratoryRateSeriesData.value, valueExtractor: (item) => item.rate, key: 'respiratoryRateValues' },
        { sourceData: oxygenData.value, valueExtractor: (item) => item.percentage, key: 'oxygenValues' }
    ]);
});

const combinedTempHeartRateProcessedData = computed(() => {
    return processCombinedData([
        { sourceData: temperatureData.value, valueExtractor: (item) => item.temperature, key: 'temperatureValues' },
        { sourceData: heartRateData.value, valueExtractor: (item) => item.bpm, key: 'heartRateValues' }
    ]);
});

const combinedGlucoseHeartRateProcessedData = computed(() => {
    return processCombinedData([
        { sourceData: bloodGlucoseSeriesData.value, valueExtractor: (item) => item.bloodGlucose, key: 'glucoseValues' },
        { sourceData: heartRateData.value, valueExtractor: (item) => item.bpm, key: 'heartRateValues' }
    ]);
});

// --- Configuración de Gráficas ---
const charts = computed(() => {
    const allCharts = [];
    const pointRadius = 3;
    const pointHoverRadius = 5;
    const lineTension = 0.2;

    const combinedBpHR = combinedPressureHeartRateProcessedData.value;
    if (combinedBpHR.labels?.length > 0 && (combinedBpHR.systolicData?.some((d) => d !== null) || combinedBpHR.heartRateData?.some((d) => d !== null))) {
        allCharts.push({
            id: 'presion_frecuencia_cardiaca',
            title: 'Presión Arterial y Frecuencia Cardíaca',
            labels: combinedBpHR.labels,
            datasets: [
                { type: 'line', label: 'Sistólica (mmHg)', borderColor: '#FF6384', backgroundColor: '#FF6384', data: combinedBpHR.systolicData, fill: false, tension: lineTension, yAxisID: 'y-bp', pointRadius, pointHoverRadius },
                { type: 'line', label: 'Diastólica (mmHg)', borderColor: '#36A2EB', backgroundColor: '#36A2EB', data: combinedBpHR.diastolicData, fill: false, tension: lineTension, yAxisID: 'y-bp', pointRadius, pointHoverRadius },
                { type: 'bar', label: 'Frec. Cardíaca (bpm)', backgroundColor: '#4CAF50', data: combinedBpHR.heartRateData, barThickness: 16, yAxisID: 'y-hr-bp' }
            ]
        });
    }

    const vitalSigns = combinedCardioRespO2ProcessedData.value;
    if (vitalSigns.labels?.length > 0 && (vitalSigns.heartRateValues?.some((d) => d !== null) || vitalSigns.respiratoryRateValues?.some((d) => d !== null) || vitalSigns.oxygenValues?.some((d) => d !== null))) {
        allCharts.push({
            id: 'signos_vitales_cardiorespiratorios',
            title: 'Signos Vitales Cardiorrespiratorios',
            labels: vitalSigns.labels,
            datasets: [
                { type: 'line', label: 'Frec. Cardíaca (bpm)', data: vitalSigns.heartRateValues, borderColor: '#E91E63', backgroundColor: '#E91E63', fill: false, tension: lineTension, yAxisID: 'y-hr-vitals', pointRadius, pointHoverRadius },
                { type: 'line', label: 'Frec. Respiratoria (rpm)', data: vitalSigns.respiratoryRateValues, borderColor: '#03A9F4', backgroundColor: '#03A9F4', fill: false, tension: lineTension, yAxisID: 'y-resp', pointRadius, pointHoverRadius },
                { type: 'line', label: 'Sat. Oxígeno (%)', data: vitalSigns.oxygenValues, borderColor: '#009688', backgroundColor: '#009688', fill: false, tension: lineTension, yAxisID: 'y-spo2', pointRadius, pointHoverRadius }
            ]
        });
    }

    const tempHr = combinedTempHeartRateProcessedData.value;
    if (tempHr.labels?.length > 0 && (tempHr.temperatureValues?.some((d) => d !== null) || tempHr.heartRateValues?.some((d) => d !== null))) {
        allCharts.push({
            id: 'termorregulacion_fc',
            title: 'Temperatura y Frecuencia Cardíaca',
            labels: tempHr.labels,
            datasets: [
                { type: 'line', label: 'Temperatura (°C)', data: tempHr.temperatureValues, borderColor: '#FF9800', backgroundColor: '#FF9800', fill: false, tension: lineTension, yAxisID: 'y-temp', pointRadius, pointHoverRadius },
                { type: 'line', label: 'Frec. Cardíaca (bpm)', data: tempHr.heartRateValues, borderColor: '#E91E63', backgroundColor: '#E91E63', fill: false, tension: lineTension, yAxisID: 'y-hr-term', pointRadius, pointHoverRadius }
            ]
        });
    }

    const glucoseHr = combinedGlucoseHeartRateProcessedData.value;
    if (glucoseHr.labels?.length > 0 && (glucoseHr.glucoseValues?.some((d) => d !== null) || glucoseHr.heartRateValues?.some((d) => d !== null))) {
        allCharts.push({
            id: 'glucosa_fc',
            title: 'Glucosa y Frecuencia Cardíaca',
            labels: glucoseHr.labels,
            datasets: [
                { type: 'line', label: 'Glucosa (mg/dL)', data: glucoseHr.glucoseValues, borderColor: '#795548', backgroundColor: '#795548', fill: false, tension: lineTension, yAxisID: 'y-glucose', pointRadius, pointHoverRadius },
                { type: 'line', label: 'Frec. Cardíaca (bpm)', data: glucoseHr.heartRateValues, borderColor: '#E91E63', backgroundColor: '#E91E63', fill: false, tension: lineTension, yAxisID: 'y-hr-glucose', pointRadius, pointHoverRadius }
            ]
        });
    }

    // Gráficas Individuales
    const pData = pressureData.value;
    if (pData.rawData?.length > 0) {
        allCharts.push({
            id: 'presion_arterial_individual',
            title: 'Presión Arterial (Detalle)',
            labels: pData.labels,
            datasets: [
                { type: 'bar', label: 'Sistólica', backgroundColor: '#FF6384', data: pData.systolicData, barThickness: 12 },
                { type: 'bar', label: 'Diastólica', backgroundColor: '#36A2EB', data: pData.diastolicData, barThickness: 12 }
            ]
        });
    }
    const hrDataI = heartRateData.value;
    if (hrDataI.rawData?.length > 0) {
        allCharts.push({
            id: 'frecuencia_cardiaca_individual',
            title: 'Frecuencia Cardíaca (Detalle)',
            labels: hrDataI.labels,
            datasets: [{ type: 'bar', label: 'Frecuencia Cardíaca (bpm)', backgroundColor: '#4CAF50', data: hrDataI.rateData, barThickness: 16 }]
        });
    }
    const oData = oxygenData.value;
    if (oData.rawData?.length > 0) {
        allCharts.push({ id: 'oxigenacion_individual', title: 'Oxigenación en Sangre (Detalle)', labels: oData.labels, datasets: [{ type: 'bar', label: 'Oxigenación (%)', backgroundColor: '#FFC107', data: oData.percentageData, barThickness: 16 }] });
    }
    const tData = temperatureData.value;
    if (tData.rawData?.length > 0) {
        allCharts.push({
            id: 'temperatura_individual',
            title: 'Temperatura Corporal (Detalle)',
            labels: tData.labels,
            datasets: [{ type: 'bar', label: 'Temperatura (°C)', backgroundColor: '#FF9800', data: tData.temperatureValues, barThickness: 16 }]
        });
    }
    const rrDataI = respiratoryRateSeriesData.value;
    if (rrDataI.rawData?.length > 0) {
        allCharts.push({
            id: 'frecuencia_respiratoria_individual',
            title: 'Frec. Respiratoria (Detalle)',
            labels: rrDataI.labels,
            datasets: [{ type: 'bar', label: 'Frec. Resp (rpm)', backgroundColor: '#03A9F4', data: rrDataI.rateValues, barThickness: 16 }]
        });
    }
    const bgDataI = bloodGlucoseSeriesData.value;
    if (bgDataI.rawData?.length > 0) {
        allCharts.push({
            id: 'glucosa_sanguinea_individual',
            title: 'Glucosa en Sangre (Detalle)',
            labels: bgDataI.labels,
            datasets: [{ type: 'bar', label: 'Glucosa (mg/dL)', backgroundColor: '#795548', data: bgDataI.glucoseValues, barThickness: 16 }]
        });
    }
    return allCharts;
});

const currentChartIndex = ref(0);

function nextChart() {
    if (charts.value.length > 0) {
        currentChartIndex.value = (currentChartIndex.value + 1) % charts.value.length;
    }
}
function prevChart() {
    if (charts.value.length > 0) {
        currentChartIndex.value = (currentChartIndex.value - 1 + charts.value.length) % charts.value.length;
    }
}

const chartOptions = ref(null);

function setChartOptions() {
    const currentChartConfig = charts.value[currentChartIndex.value];
    const textColor = '#5e6577';
    const gridColor = '#e9ecef';
    const titleColor = '#343a40';

    const options = {
        maintainAspectRatio: false,
        aspectRatio: 0.75,
        responsive: true,
        animation: { duration: 400, easing: 'easeInOutQuad' },
        scales: {
            x: {
                stacked: false,
                ticks: { color: textColor, font: { size: 10 } },
                grid: { display: false }
            },
            y: {
                display: true,
                stacked: false,
                ticks: { color: textColor, font: { size: 10 }, padding: 8 },
                grid: { color: gridColor, borderColor: gridColor, drawTicks: false },
                beginAtZero: true
            }
        },
        plugins: {
            legend: { display: true, labels: { color: titleColor, font: { size: 12 }, usePointStyle: true, boxWidth: 10, padding: 20 }, position: 'bottom' },
            tooltip: {
                enabled: true,
                mode: 'index',
                intersect: false,
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                titleFont: { size: 13, weight: 'bold' },
                bodyFont: { size: 12 },
                padding: 12,
                cornerRadius: 4,
                displayColors: true,
                callbacks: {
                    label: function (context) {
                        let label = context.dataset.label || '';
                        if (label) {
                            label += ': ';
                        }
                        if (context.parsed.y !== null) {
                            if (typeof context.parsed.y === 'number' && !Number.isInteger(context.parsed.y)) {
                                label += context.parsed.y.toFixed(1);
                            } else {
                                label += context.parsed.y;
                            }
                        }
                        return label;
                    }
                }
            }
        },
        interaction: { mode: 'nearest', axis: 'x', intersect: false },
        elements: { line: { tension: 0.3 }, point: { radius: 0, hoverRadius: 5, hitRadius: 10 } }
    };

    ['y-bp', 'y-hr-bp', 'y-hr-vitals', 'y-resp', 'y-spo2', 'y-temp', 'y-hr-term', 'y-glucose', 'y-hr-glucose'].forEach((key) => delete options.scales[key]);
    options.scales.y.display = true;

    if (currentChartConfig) {
        const makeAxis = (position, titleText, tickColor, suggestedMin, suggestedMax, gridDraw = false, precision = 0, offset = false) => ({
            type: 'linear',
            display: true,
            position,
            offset,
            title: { display: true, text: titleText, color: titleColor, font: { size: 11, weight: 'bold' } },
            ticks: { color: tickColor, font: { size: 10 }, padding: 5, precision: precision },
            grid: { drawOnChartArea: gridDraw, color: gridColor, borderColor: gridColor, drawTicks: false },
            ...(suggestedMin !== undefined && { suggestedMin }),
            ...(suggestedMax !== undefined && { suggestedMax })
        });

        if (currentChartConfig.id === 'presion_frecuencia_cardiaca') {
            options.scales['y-bp'] = makeAxis('left', 'Presión (mmHg)', '#FF6384', 50, 200, true);
            options.scales['y-hr-bp'] = makeAxis('right', 'BPM', '#4CAF50', 40, 160);
            options.scales.y.display = false;
        } else if (currentChartConfig.id === 'signos_vitales_cardiorespiratorios') {
            options.scales['y-hr-vitals'] = makeAxis('left', 'BPM', '#E91E63', 40, 180, true);
            options.scales['y-resp'] = makeAxis('right', 'RPM', '#03A9F4', 5, 35);
            options.scales['y-spo2'] = makeAxis('right', 'SpO2 %', '#009688', 85, 100, false, 0, true); // offset true
            options.scales.y.display = false;
        } else if (currentChartConfig.id === 'termorregulacion_fc') {
            options.scales['y-temp'] = makeAxis('left', 'Temp (°C)', '#FF9800', 35, 41, true, 1);
            options.scales['y-hr-term'] = makeAxis('right', 'BPM', '#E91E63', 40, 180);
            options.scales.y.display = false;
        } else if (currentChartConfig.id === 'glucosa_fc') {
            options.scales['y-glucose'] = makeAxis('left', 'Glucosa (mg/dL)', '#795548', 50, 250, true);
            options.scales['y-hr-glucose'] = makeAxis('right', 'BPM', '#E91E63', 40, 180);
            options.scales.y.display = false;
        } else {
            options.scales.y.beginAtZero = !['temperatura_individual', 'oxigenacion_individual'].includes(currentChartConfig.id);
            options.scales.y.ticks.precision = 0;

            if (currentChartConfig.id === 'temperatura_individual') {
                options.scales.y.suggestedMin = 35;
                options.scales.y.ticks.precision = 1;
            }
            if (currentChartConfig.id === 'oxigenacion_individual') {
                options.scales.y.suggestedMin = 85;
                options.scales.y.suggestedMax = 100;
            }
        }
    }
    return options;
}

const downloadChart = () => {
    if (chartComponentRef.value && charts.value.length > 0 && charts.value[currentChartIndex.value]) {
        const chartInstance = chartComponentRef.value.getChart();
        if (chartInstance) {
            const base64Image = chartInstance.toBase64Image();
            const link = document.createElement('a');
            const currentChartData = charts.value[currentChartIndex.value];
            const chartTitleForFile = (
                currentChartData.id ||
                currentChartData.title
                    .toLowerCase()
                    .replace(/\s+/g, '_')
                    .replace(/[^\w-]+/g, '')
            ).substring(0, 50);
            const selectedDateString = formatDate(chartSelectedDate.value) || 'fecha-no-sel';
            link.download = `${chartTitleForFile}_${selectedDateString}.png`;
            link.href = base64Image;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        } else {
            console.error('No se pudo obtener la instancia de la gráfica.');
        }
    } else {
        console.error('Referencia a la gráfica no disponible o no hay gráficas para descargar.');
    }
};

onMounted(() => {
    // Las opciones y el primer renderizado se manejan a través del watch con immediate: true
});

// Watcher para actualizar las opciones del gráfico y redimensionar
watch(
    [currentChartIndex, chartSelectedDate, dias], // Observar estas dependencias
    async () => {
        if (charts.value.length > 0 && charts.value[currentChartIndex.value]) {
            chartOptions.value = setChartOptions();

            await nextTick(); // Esperar a que el DOM se actualice

            if (chartComponentRef.value) {
                const chartInstance = chartComponentRef.value.getChart();
                if (chartInstance) {
                    chartInstance.resize();
                    // chartInstance.update('none'); // 'none' para evitar re-animación si solo es resize
                }
            }
        } else {
            chartOptions.value = null;
        }
    },
    { deep: true, immediate: true } // immediate: true para la carga inicial
);
</script>
<template>
    <div class="flex flex-col">
        <div class="mb-4 flex justify-center">
            <Calendar
                v-model="chartSelectedDate"
                dateFormat="dd/mm/yy"
                :minDate="availableDates.length > 0 ? availableDates[availableDates.length - 1] : null"
                :maxDate="availableDates.length > 0 ? availableDates[0] : new Date()"
                :disabled="availableDates.length === 0"
                :showIcon="true"
                :manualInput="false"
                panelClass="shadow-lg rounded-md"
                @date-select="
                    () => {
                        currentChartIndex.value = 0; // Corregido para usar .value
                        chartOptions.value = setChartOptions(); // Corregido para usar .value
                    }
                "
                :pt="{
                    input: { class: 'shadow-md border-gray-300 rounded-md p-2 focus:ring-blue-500 focus:border-blue-500' },
                    panel: { class: 'bg-white border-gray-200 rounded-lg shadow-xl' },
                    header: { class: 'text-gray-800' },
                    dayLabel: { class: 'text-gray-600' }
                }"
            >
                <template #date="slotProps">
                    <template v-if="slotProps.date && slotProps.date.day">
                        <strong
                            v-if="isDateSelectable(new Date(slotProps.date.year, slotProps.date.month, slotProps.date.day))"
                            style="text-decoration: underline; color: #007bff; display: inline-block; width: 100%; height: 100%; text-align: center; line-height: 2.5rem"
                        >
                            {{ slotProps.date.day }}
                        </strong>
                        <span v-else style="display: inline-block; width: 100%; height: 100%; text-align: center; line-height: 2.5rem">
                            {{ slotProps.date.day }}
                        </span>
                    </template>
                </template>
            </Calendar>
        </div>
        <div class="card flex flex-col sm:flex-row items-center p-4 bg-white shadow-xl rounded-lg">
            <button @click="prevChart" class="p-3 m-2 bg-gray-200 hover:bg-gray-300 text-gray-700 font-bold rounded-full shadow-md transition-colors duration-150" :disabled="!charts || charts.length <= 1" aria-label="Previous chart">⬅️</button>
            <div class="flex-1 w-full min-w-0 relative" v-if="charts && charts.length > 0 && charts[currentChartIndex]">
                <div class="font-semibold text-xl mb-2 text-center text-gray-700">{{ charts[currentChartIndex].title }}</div>
                <button
                    @click="downloadChart"
                    class="absolute top-0 right-0 mt-1 mr-1 p-2 bg-blue-500 hover:bg-blue-600 text-white text-xs font-bold rounded-md shadow-md transition-colors duration-150 z-10"
                    aria-label="Descargar gráfica"
                    title="Descargar gráfica"
                    :disabled="!charts || charts.length === 0 || !chartComponentRef"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                    </svg>
                </button>
                <div class="relative w-full h-80 sm:h-96">
                    <Chart ref="chartComponentRef" type="bar" :data="charts[currentChartIndex]" :options="chartOptions" class="w-full h-full" />
                </div>
            </div>
            <div class="flex-1 w-full text-center p-10" v-else>
                <div class="font-semibold text-xl mb-4 text-gray-500">
                    <span v-if="availableDates.length === 0 && !chartSelectedDate">No hay datos cargados.</span>
                    <span v-else-if="!chartSelectedDate">Por favor, selecciona una fecha.</span>
                    <span v-else>No hay datos de gráficos disponibles para la fecha seleccionada.</span>
                </div>
                <svg class="mx-auto h-24 w-24 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                    <path
                        vector-effect="non-scaling-stroke"
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V7c0-1.1.9-2 2-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                    />
                </svg>
            </div>
            <button @click="nextChart" class="p-3 m-2 bg-gray-200 hover:bg-gray-300 text-gray-700 font-bold rounded-full shadow-md transition-colors duration-150" :disabled="!charts || charts.length <= 1" aria-label="Next chart">➡️</button>
        </div>
    </div>
</template>

<style scoped>
.card {
    max-width: 100%;
    /* overflow: hidden; /* Comentado o cambiado a visible para que el botón de descarga no se corte */
    overflow: visible;
}

@media (max-width: 640px) {
    .card {
        flex-direction: column;
    }
    .card button:not(.absolute) {
        margin: 8px 0;
    }
    .card .relative.w-full {
        /* Contenedor de la gráfica y su título */
        /* height: 250px;  No es necesario si el div interno de la gráfica tiene altura fija */
    }
    .card .relative.w-full .relative.w-full {
        /* Contenedor directo de la gráfica */
        height: 250px !important; /* Forzar altura en móviles si es necesario */
    }
}
</style>
