<script setup>
    import Header from './Header.vue'
    import Banner from './Banner.vue'
    import Chart from './Chart.vue'
    import ProductTable from './ProductTable.vue'
    import Footer from './Footer.vue'
    import { ref, onMounted } from 'vue'

    const historyData = ref(null)  // HistoryResponse

    onMounted(async () => {
        try {
            const res = await fetch('/api/refresh')
            const data = await res.json()
            historyData.value = data  // HistoryResponse
            console.log('Полные данные:', historyData.value)
        } catch (err) {
            console.error('Ошибка загрузки:', err)
        }
    })
</script>

<template>
    <div>
        <Header />
        <Banner />
        <Chart />
        <ProductTable :data="historyData" />
        <Footer />
    </div>
</template>