<template>
  <div>
    <line-chart :key="chartKey" v-if="chartData" :chart-data="chartData" :options="chartOptions" />

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
  import LineChart from '~/components/BarChart.vue'

  export default {
    components: {
      LineChart
    },
    data() {
      return {
        startDate: '',
        endDate: '',
        granularity: 'day',
        chartData: {
          labels: [],
          datasets: [{
            label: 'Number of Completed Requests',
            data: [],
            backgroundColor: 'rgba(75,192,192,0.4)',
            borderColor: 'rgba(75,192,192,1)',
            borderWidth: 1,
            fill: false
          }]
        },
        chartOptions: {
          responsive: true,
          maintainAspectRatio: false
        },
        chartKey: 0
      }
    },
    methods: {
      async fetchData() {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/request/getnumberofcompletedrequestssummary?startDate=${this.startDate}&endDate=${this.endDate}&granularity=${this.granularity}`);
        const responseData = response.data.data;

        this.$set(this.chartData, 'labels', responseData.map(item => item.period));
        this.$set(this.chartData.datasets[0], 'data', responseData.map(item => item.completedRequests));

        this.chartKey++;
      }
    },
    created() {
      const now = new Date();
      const timezoneOffset = now.getTimezoneOffset() * 60000;
      const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
      this.startDate = new Date(startOfMonth - timezoneOffset).toISOString().split('T')[0];

      const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      this.endDate = new Date(endOfMonth.getTime() - timezoneOffset).toISOString().split('T')[0];

    }
  }
</script>
