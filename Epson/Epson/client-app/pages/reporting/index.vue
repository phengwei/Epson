<template>
  <div>
    <div class="chart-container">
      <div class="card">
        <div class="card-header">Monthly request by Requester</div>
        <div class="card-body">
          <select v-model="selectedRequester" @change="fetchmonthlysalesbyrequester">
            <option value="all">All Requesters</option>
            <option v-for="requester in requesters" :value="requester.id" :key="requester.id">{{ requester.userName }}</option>
          </select>
          <div class="bar-chart-container">
            <bar-chart :chart-data="monthlysalesbyrequester" :options="options"></bar-chart>
          </div>
        </div>
      </div>
      <div class="card">
        <div class="card-header">Top 10 Requesters by request count</div>
        <div class="card-body">
          <select v-model="selectedMonth_requesterBySales" @change="fetchtoprequestersbysales">
            <option value="0">All Months</option>
            <option v-for="month in months" :value="month.value" :key="month.value">{{ month.text }}</option>
          </select>
          <div class="bar-chart-container">
            <bar-chart :chart-data="toprequestersbysales" :options="options"></bar-chart>
          </div>
        </div>
      </div>
      <div class="card">
        <div class="card-header">Top 10 Products By request Count</div>
        <div class="card-body">
          <select v-model="selectedMonth_productsByRevenue" @change="fetchtopproductsbyrevenue">
            <option value="0">All Months</option>
            <option v-for="month in months" :value="month.value" :key="month.value">{{ month.text }}</option>
          </select>
          <div class="bar-chart-container">
            <bar-chart :chart-data="topproductsbyrevenue" :options="options"></bar-chart>
          </div>
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
        selectedRequester: 'all',
        selectedMonth_requesterBySales: '0',
        selectedMonth_productsByRevenue: '0',
        requesters: [],
        months: [
          { text: 'January', value: '1' },
          { text: 'February', value: '2' },
          { text: 'March', value: '3' },
          { text: 'April', value: '4' },
          { text: 'May', value: '5' },
          { text: 'June', value: '6' },
          { text: 'July', value: '7' },
          { text: 'August', value: '8' },
          { text: 'September', value: '9' },
          { text: 'October', value: '10' },
          { text: 'November', value: '11' },
          { text: 'December', value: '12' }
        ]
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
        const monthParam = this.selectedMonth_requesterBySales;
        const response = await this.$axios.get(`${this.$config.restUrl}/api/report/gettoprequestersbysales?month=${monthParam}`);

        const labels = response.data.data.map(({ requesterName }) => requesterName);
        const data = response.data.data.map(({ totalNumberOfSales }) => totalNumberOfSales);

        this.toprequestersbysales = {
          labels,
          datasets: [
            {
              label: 'Top Requesters by Request Count',
              backgroundColor: '#f87979',
              data
            }
          ]
        }
      },
      async fetchtopproductsbyrevenue() {
        const monthParam = this.selectedMonth_productsByRevenue;
        const response = await this.$axios.get(`${this.$config.restUrl}/api/report/gettopproductsbyrevenue?month=${monthParam}`);

        const abbreviateProductName = (name) => {
          if (name.length > 6) {
            return name.slice(0, 5) + '...';
          }
          return name;
        };

        const productNamesMap = {};
        const labels = response.data.data.map(({ productName }) => {
          const abbreviated = abbreviateProductName(productName);
          productNamesMap[abbreviated] = productName;
          return abbreviated;
        });
        const data = response.data.data.map(({ totalNoOfSales }) => totalNoOfSales);

        this.topproductsbyrevenue = {
          labels,
          datasets: [
            {
              label: 'Top Products by Request',
              backgroundColor: '#f87979',
              data
            }
          ]
        };

        this.options = {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            yAxes: [{
              ticks: {
                beginAtZero: true
              }
            }]
          },
          tooltips: {
            callbacks: {
              title: (tooltipItems) => {
                const abbreviatedName = tooltipItems[0].label;
                return productNamesMap[abbreviatedName] || abbreviatedName;
              }
            }
          }
        };
      },
      async fetchmonthlysalesbyrequester() {
        let response;
        if (this.selectedRequester === 'all') {
          response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyrequester?allRequester=true`);
        } else {
          response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyrequester?requesterId=${this.selectedRequester}`);
        }

        const monthlySalesData = Array(12).fill(0);

        response.data.data.forEach(salesData => {
          const monthNumber = parseInt(salesData.month.split('-')[1], 10);

          monthlySalesData[monthNumber - 1] = salesData.totalNumberOfSales;
        });

        this.monthlysalesbyrequester = {
          labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
          datasets: [
            {
              label: 'Monthly Request Count',
              backgroundColor: '#f87979',
              data: monthlySalesData
            }
          ]
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
    flex-wrap: wrap;
    justify-content: space-between;
    gap: 20px;
    width: 90%;
    margin: 0 auto;
  }

  .card {
    flex: 1 1 30%;
    background-color: #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    border-radius: 4px;
  }

  .card-header {
    padding: 12px;
    background-color: #f5f5f5;
    border-bottom: 1px solid #ddd;
    font-weight: bold;
    text-align: center; 
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
    height: 380px;
  }

  @media (max-width: 768px) {
    .chart-container {
      flex-direction: column; 
    }

    .card {
      flex: 1 1 100%; 
      margin-bottom: 20px; 
    }
  }
</style>
