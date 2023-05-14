<template>
  <div v-if="isClient && readyToRender" class="flex justify-center items-center min-h-screen">
    <div class="flex justify-around w-full max-w-4xl">
      <div class="chart-card">
        <apexchart type="pie"
                   :options="chartOptions1"
                   :series="[44, 55, 13]" />
      </div>
      <div class="chart-card">
        <apexchart type="pie"
                   :options="chartOptions2"
                   :series="[25, 15, 60]" />
      </div>
      <div class="chart-card">
        <apexchart type="pie"
                   :options="chartOptions3"
                   :series="[30, 40, 30]" />
      </div>
    </div>
  </div>
</template>

<script>
  import VueApexCharts from 'vue-apexcharts'
  import axios from 'axios';

  export default {
    name: 'PieChart',
    components: {
      apexchart: VueApexCharts,
    },
    computed: {
      isClient() {
        return process.client
      },
    },
    data() {
      return {
        chartOptions1: {
          labels: ['Label 1a', 'Label 2a', 'Label 3a'],
          responsive: [{
            breakpoint: 480,
            options: {
              chart: {
                width: 200,
              },
              legend: {
                position: 'bottom',
              },
            },
          }],
        },
        chartOptions2: {
          labels: ['Label 1b', 'Label 2b', 'Label 3b'],
          responsive: [{
            breakpoint: 480,
            options: {
              chart: {
                width: 200,
              },
              legend: {
                position: 'bottom',
              },
            },
          }],
        },
        chartOptions3: {
          labels: ['Label 1c', 'Label 2c', 'Label 3c'],
          responsive: [{
            breakpoint: 480,
            options: {
              chart: {
                width: 200,
              },
              legend: {
                position: 'bottom',
              },
            },
          }],
        },
        readyToRender: false,
        monthlySales: [],
        topRequesters: [],
        topProducts: [],
      }
    },
    methods: {
      fetchMonthlySales() {
        axios.get('/api/report/getmonthlysalesbyrequester')
          .then(response => {
            this.monthlySales = response.data;
            console.log("monthly sales", this.monthlySales);
          })
          .catch(error => {
            console.log(error);
          });
      },
      fetchTopRequesters() {
        axios.get('/api/report/gettoprequestersbysales')
          .then(response => {
            this.topRequesters = response.data;
          })
          .catch(error => {
            console.log(error);
          });
      },
      fetchTopProducts() {
        axios.get('/api/report/gettopproductsbyrevenue')
          .then(response => {
            this.topProducts = response.data;
          })
          .catch(error => {
            console.log(error);
          });
      },
    },
    mounted() {
      this.$nextTick(() => {
        this.fetchMonthlySales();
        this.fetchTopRequesters();
        this.fetchTopProducts();
        this.readyToRender = true;
      });
    },
  }
</script>

<style>
  .chart-card {
    flex: 1;
    max-width: 30%;
    border: 1px solid #e2e8f0;
    border-radius: 4px;
    padding: 1rem;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
</style>
