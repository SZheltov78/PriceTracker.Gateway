<template>
    <div class="container my-4">
        <div class="table-responsive">
            <table class="table table-striped table-hover">
                <thead>
                    <tr>
                        <th>Название</th>
                        <th class="d-none d-md-table-cell">URL</th>
                        <th class="d-none d-lg-table-cell">Статус</th>
                        <th>Действие</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in products" :key="item.url" @click="viewDetails(item)" style="cursor: pointer">
                        <td>{{ item.productName }}</td>
                        <td class="d-none d-md-table-cell">{{ truncateUrl(item.url) }}</td>
                        <td class="d-none d-lg-table-cell">{{ item.status }}</td>
                        <td>
                            <button class="btn btn-sm btn-primary" @click.stop="viewDetails(item)">
                                График
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup>
    import { computed } from 'vue'

    const props = defineProps({
        data: {
            type: Object,
            default: () => ({ products: [] })
        }
    })

    const products = computed(() => props.data?.products || [])

    const truncateUrl = (url) => {
        if (!url) return '—'
        return url.length > 10 ? url.substring(0, 10) + '...' : url
    }

    const viewDetails = (item) => {
        console.log('График для:', item.productName)
    }
</script>

<style scoped>
    .table-hover tbody tr:hover {
        background-color: rgba(0, 0, 0, 0.075);
    }
</style>