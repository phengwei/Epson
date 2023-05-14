<template>
  <div>
    <div class="chart-container">
      <div class="card">
        <div class="card-header">Monthly Sales by Requester</div>
        <div class="card-body">
          <select v-model="selectedRequester" @change="fetchmonthlysalesbyrequester">
            <option value="">Select Requester</option>
            <option v-for="requester in requesters" :value="requester.id" :key="requester.id">{{ requester.userName }}</option>
          </select>
          <bar-chart :chart-data="monthlysalesbyrequester" :options="options"></bar-chart>
        </div>
      </div>
      <div class="card">
        <div class="card-header">Top 10 Requesters by Sales</div>
        <div class="card-body">
          <bar-chart :chart-data="toprequestersbysales" :options="options"></bar-chart>
        </div>
      </div>
      <div class="card">
        <div class="card-header">Top 10 Products By Revenue</div>
        <div class="card-body">
          <bar-chart :chart-data="topproductsbyrevenue" :options="options"></bar-chart>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
  import BarChart from '~/components/BarChart.vue'

  export default {
    name: 'ReportingDashboard',
    data() {
      return {
        monthlysalesbyrequester: null,
        toprequestersbysales: null,
        topproductsbyrevenue: null,
        datacollection: null,
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            yAxes: [{
              ticks: {
                beginAtZero: true
              }
            }]
          }
        },
        selectedRequester: '',
        requesters: []
      }
    },
    async created() {
      await this.fetchRequesters();
      await this.fetchmonthlysalesbyrequester();
      await this.fetchtoprequestersbysales();
      await this.fetchtopproductsbyrevenue();
    },
    methods: {
      async fetchRequesters() {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/customer/getallrequesters`);
        this.requesters = response.data.data;
      },
      async fetchtoprequestersbysales() {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/report/gettoprequestersbysales`);

        const labels = response.data.data.map(({ requesterName }) => requesterName);
        const data = response.data.data.map(({ totalSales }) => totalSales);

        this.toprequestersbysales = {
          labels,
          datasets: [
            {
              label: 'Top Requesters by Sales',
              backgroundColor: '#f87979',
              data
            }
          ]
        }
      },
      async fetchtopproductsbyrevenue() {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/report/gettopproductsbyrevenue`);

        const labels = response.data.data.map(({ productName }) => productName);
        const data = response.data.data.map(({ totalRevenue }) => totalRevenue);

        this.topproductsbyrevenue = {
          labels,
          datasets: [
            {
              label: 'Top Products by Revenue',
              backgroundColor: '#f87979',
              data
            }
          ]
        }
      },
      async fetchmonthlysalesbyrequester() {
        if (this.selectedRequester) {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyrequester?requesterId=${this.selectedRequester}`);

          const monthlySalesData = Array(12).fill(0);

          response.data.data.forEach(salesData => {
            const monthNumber = parseInt(salesData.month.split('-')[1], 10);

            monthlySalesData[monthNumber - 1] = salesData.monthlySales;
          });

          this.monthlysalesbyrequester = {
            labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
            datasets: [
              {
                label: 'Monthly Sales',
                backgroundColor: '#f87979',
                data: monthlySalesData
              }
            ]
          }
        }
      }
    },
    components: {
      BarChart
    }
  }
</script>

<style scoped>
  .chart-container {
    display: flex;
    justify-content: center;
    gap: 20px;
  }

  .card {
    width: 300px;
    background-color: #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    border-radius: 4px;
  }

  .card-header {
    padding: 12px;
    background-color: #f5f5f5;
    border-bottom: 1px solid #ddd;
    font-weight: bold;
  }

  .card-body {
    padding: 12px;
  }

  select {
    width: 100%;
    margin-bottom: 10px;
    padding: 8px;
    font-size: 14px;
    border: 1px solid #ccc;
    border-radius: 4px;
    box-sizing: border-box;
  }

  .bar-chart-container {
    height: 300px;
  }
</style>
