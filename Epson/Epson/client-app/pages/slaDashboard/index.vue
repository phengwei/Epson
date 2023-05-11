<template>
  <div class="page">
    <div class="container">
        <div class="card">
          <div class="mini-card">
            <h3 class="number">{{ AverageTimeToResolutionInHours }}</h3>
            <p>Average time to resolution in hours:</p>
          </div>
          <div class="mini-card">
            <h3 class="number">{{ TotalTickets }}</h3>
            <p>Total Tickets</p>
          </div>
        </div>
        <div class="card">
          <div class="breached-card">
            <h3 class="number">{{ BreachedTickets }}</h3>
            <p>Breached Ticket</p>
          </div>
          <div class="mini-card">
            <h3 class="number">{{ SuccessRate }}%</h3>
            <p>Success Rate</p>
          </div>
        </div>
    </div>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  export default {
    name: 'SLA-Dashboard',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        AverageTimeToResolutionInHours: 0,
        BreachedTickets: 0,
        TotalTickets: 0,
        SuccessRate: 0
      };
    },
    methods: {
      async getSLAMetrics() {
        try {
          this.loading = true
          await this.$axios.get(`${this.$config.restUrl}/api/sla/getslametrics`).then(result => {
            this.AverageTimeToResolutionInHours = result.data.data.averageTimeToResolutionInHours
            this.BreachedTickets = result.data.data.breachedTickets
            this.TotalTickets = result.data.data.totalTickets
            this.SuccessRate = result.data.data.successRate

          })
        } catch (error) {
          console.error('There was a problem fetching the SLA metrics:', error);
        }
      }
    },
    mounted() {
      this.getSLAMetrics();
      console.log(this.loggedInUser)
    }
  };
</script>

<style>
  .page {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
  }

  .container {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: 100vh;
  }

  .overview {
    text-align: center;
    margin-bottom: 1rem;
  }

    .overview h1 {
      font-size: 24px;
      margin-bottom: 0.5rem;
    }

  .card {
    background-color: #fff;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 1rem;
    margin: 0 0.5rem;
    flex: 1;
    height: 80%;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }

  .mini-card,
  .breached-card {
    background-color: #f0f0f0;
    border: 1px solid #ddd;
    border-radius: 4px;
    padding: 0.5rem;
    margin-bottom: 0.5rem;
    height: 50%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    color: white;
  }

  .breached-card {
    background-color: #9b5050;
    color: white;
  }

    .breached-card p {
      font-size: 18px;
      color: white;
    }

    .mini-card .number,
    .breached-card .number {
      font-size: 50px;
      font-weight: bold;
      margin-bottom: 0.5rem;
    }

  .mini-card p {
    font-size: 18px;
    color: #102381b3;
  }
</style>
