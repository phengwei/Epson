<template>
  <div class="page">
    <div class="filter-bar">
      <label for="month-select">Select Month:</label>
      <select v-model="selectedMonth" id="month-select">
        <option v-for="month in months" :key="month.value" :value="month.value">{{ month.text }}</option>
      </select>
    </div>
    <div class="container">
      <div class="card average-time-card">
        <h3 class="number">{{ AverageTimeToResolutionInHours }}</h3>
        <p>Average time to resolution in hours</p>
      </div>
      <div class="card total-tickets-card">
        <h3 class="number">{{ TotalTickets }}</h3>
        <p>Total Requests</p>
      </div>
      <div class="card breached-card">
        <h3 class="number">{{ BreachedTickets }}</h3>
        <p>Breached Requests</p>
        <button @click="goToBreachedTickets" class="breached-button">Go to Breached Tickets</button>
      </div>
      <div class="card success-rate-card">
        <h3 class="number">{{ SuccessRate }}%</h3>
        <p>Success Rate</p>
      </div>
    </div>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';

  export default {
    name: 'SLA-Dashboard',
    middleware: 'auth',
    data() {
      return {
        AverageTimeToResolutionInHours: 0,
        BreachedTickets: 0,
        TotalTickets: 0,
        SuccessRate: 0,
        selectedMonth: new Date().getMonth() + 1,
        months: [
          { value: 0, text: 'All' },
          { value: 1, text: 'January' },
          { value: 2, text: 'February' },
          { value: 3, text: 'March' },
          { value: 4, text: 'April' },
          { value: 5, text: 'May' },
          { value: 6, text: 'June' },
          { value: 7, text: 'July' },
          { value: 8, text: 'August' },
          { value: 9, text: 'September' },
          { value: 10, text: 'October' },
          { value: 11, text: 'November' },
          { value: 12, text: 'December' }
        ],
        loading: false
      };
    },
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    watch: {
      selectedMonth(newMonth) {
        this.getSLAMetrics(newMonth);
      }
    },
    mounted() {
      this.getSLAMetrics();
    },
    methods: {
      async getSLAMetrics(month = this.selectedMonth) {
        try {
          this.loading = true;
          const result = await this.$axios.get(`${this.$config.restUrl}/api/sla/getslametrics`, {
            params: { month }
          });

          if (result.data.data) {
            this.AverageTimeToResolutionInHours = result.data.data.averageTimeToResolutionInHours || 0;
            this.BreachedTickets = result.data.data.breachedTickets || 0;
            this.TotalTickets = result.data.data.totalTickets || 0;
            this.SuccessRate = result.data.data.successRate || 0;
            this.$forceUpdate()
          } else {
            this.resetMetrics();
          }
        } catch (error) {
          console.error('There was a problem fetching the SLA metrics:', error);
          this.resetMetrics();
        } finally {
          this.loading = false;
        }
      },
      resetMetrics() {
        this.AverageTimeToResolutionInHours = 0;
        this.BreachedTickets = 0;
        this.TotalTickets = 0;
        this.SuccessRate = 0;
        console.log('Metrics reset.');
      },
      goToBreachedTickets() {
        this.$router.push({ path: '/request', query: { breached: true, month: this.selectedMonth } });
      }
    }
  };
</script>


<style scoped>
  .page {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100vh;
    padding: 1rem;
  }

  .filter-bar {
    margin-bottom: 1rem;
    display: flex;
    align-items: center;
  }

    .filter-bar label {
      margin-right: 0.5rem;
      font-weight: bold;
    }

    .filter-bar select {
      padding: 0.5rem;
      font-size: 1rem;
    }

  .container {
    display: grid;
    grid-template-columns: 1fr 1fr;
    grid-template-rows: 1fr 1fr;
    gap: 1rem;
    width: 100%;
    height: calc(100vh - 3rem);
  }

  .card {
    background-color: #fff;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 2rem;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  }

  .average-time-card {
    background-color: #4caf50;
    color: white;
  }

  .total-tickets-card {
    background-color: #2196f3;
    color: white;
  }

  .breached-card {
    background-color: #9b5050;
    color: white;
  }

  .success-rate-card {
    background-color: #ff9800;
    color: white;
  }

  .number {
    font-size: 4rem;
    font-weight: bold;
    margin-bottom: 1rem;
  }

  .breached-button {
    background-color: #fff;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 0.5rem 1rem;
    cursor: pointer;
    color: #9b5050;
    font-size: 1rem;
    font-weight: bold;
    margin-top: 1rem;
  }

    .breached-button:hover {
      background-color: #f2dede;
    }
</style>
