<template>
  <div>
    <bar-chart v-if="chartData" :chart-data="chartData" :options="chartOptions" />

    <div>
      <label>Start Date: <input type="date" v-model="startDate" /></label>
      <label>End Date: <input type="date" v-model="endDate" /></label>
      <label>
        By:
        <select v-model="granularity">
          <option value="day">Day</option>
          <option value="week">Week</option>
          <option value="month">Month</option>
        </select>
      </label>
      <button @click="fetchData">Fetch Data</button>
    </div>
  </div>
</template>

<script>
  import BarChart from '~/components/BarChart.vue'

  export default {
    name: 'FulfillmentRequestSummary',
    components: {
      BarChart
    },
    data() {
      return {
        startDate: '2023-05-01',
        endDate: '2023-12-31',
        granularity: 'day',
        chartData: null,
        chartOptions: {
          responsive: true,
          maintainAspectRatio: false
        }
      }
    },
    methods: {
      async fetchData() {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/request/getfulfillmentsummary?startDate=${this.startDate}&endDate=${this.endDate}&granularity=${this.granularity}`);
        const responseData = response.data.data;

        const chartData = {
          labels: responseData.map(item => item.period),  
          datasets: [
            {
              label: 'Fulfillments',
              data: responseData.map(item => item.fulfillments),
              backgroundColor: 'rgba(75,192,192,0.4)', 
              borderColor: 'rgba(75,192,192,1)',
              borderWidth: 1
            }
          ]
        };

        this.chartData = chartData;
      }
    },
    created() {
      this.fetchData()
    }
  }
</script>
