<template>
  <v-app>
    <div class="page">
      <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
        <v-toolbar flat>
          <v-toolbar-title>
            <h2 class="blue-text big-bold">SLA OVERVIEW</h2>
          </v-toolbar-title>
        </v-toolbar>
        <v-card-text>
          <div class="filter-bar">
            <label for="month-select">Select Month:</label>
            <v-select v-model="selectedMonth"
                      :items="months"
                      item-text="text"
                      item-value="value"
                      outlined
                      dense
                      class="month-select"></v-select>
          </div>
          <div class="container">
            <div class="card average-time-card">
              <div class="card-content">
                <v-icon class="top-center-icon">mdi-clock-outline</v-icon>
                <h2 class="number">{{ AverageTimeToResolutionInHours }}</h2>
                <p class="bottom-center-text">Hours Average Time To Resolution</p>
                <div class="bottom-reserved-space"></div>
              </div>
            </div>
            <div class="card total-tickets-card">
              <div class="card-content">
                <v-icon class="top-center-icon">mdi-ticket-outline</v-icon>
                <h2 class="number">{{ TotalTickets }}</h2>
                <p class="bottom-center-text">Total Tickets</p>
                <div class="bottom-reserved-space"></div>
              </div>
            </div>
            <div class="card breached-card">
              <div class="card-content">
                <v-icon class="top-center-icon">mdi-alert-circle-outline</v-icon>
                <h2 class="number">{{ BreachedTickets }}</h2>
                <p class="bottom-center-text">Breached Tickets</p>
                <div class="bottom-reserved-space">
                  <v-btn class="navigate-button" @click="goToBreachedTickets">
                    Navigate
                    <v-icon right>mdi-arrow-right-circle-outline</v-icon>
                  </v-btn>
                </div>
              </div>
            </div>
            <div class="card success-rate-card">
              <div class="card-content">
                <v-icon class="top-center-icon">mdi-trophy-outline</v-icon>
                <h2 class="number">{{ SuccessRate }}%</h2>
                <p class="bottom-center-text">Success Rate</p>
                <div class="bottom-reserved-space"></div>
              </div>
            </div>
          </div>
        </v-card-text>
      </v-card>
    </div>
  </v-app>
</template>

<script>
  import { VIcon, VSelect, VBtn, VCard, VToolbar, VToolbarTitle, VCardText } from 'vuetify/lib';

  export default {
    name: 'SLA-Dashboard',
    components: {
      VIcon,
      VSelect,
      VBtn,
      VCard,
      VToolbar,
      VToolbarTitle,
      VCardText,
    },
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
          { value: 12, text: 'December' },
        ],
      };
    },
    watch: {
      selectedMonth(newMonth) {
        this.getSLAMetrics(newMonth);
      },
    },
    mounted() {
      this.getSLAMetrics();
    },
    methods: {
      async getSLAMetrics(month = this.selectedMonth) {
        try {
          const result = await this.$axios.get(`${this.$config.restUrl}/api/sla/getslametrics`, {
            params: { month },
          });

          if (result.data.data) {
            this.AverageTimeToResolutionInHours = result.data.data.averageTimeToResolutionInHours || 0;
            this.BreachedTickets = result.data.data.breachedTickets || 0;
            this.TotalTickets = result.data.data.totalTickets || 0;
            this.SuccessRate = result.data.data.successRate || 0;
          } else {
            this.resetMetrics();
          }
        } catch (error) {
          console.error('There was a problem fetching the SLA metrics:', error);
          this.resetMetrics();
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
      },
    },
  };
</script>
<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
  .page {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 1rem;
  }
  .filter-bar {
    margin: auto;
    margin-bottom: 1rem;
    display: flex;
    justify-content: center;
    width: 35%;
  }

    .filter-bar label {
      margin-right: 0.5rem;
      font-weight: bold;
    }

  .month-select {
    width: 100%;
  }

  .container {
    display: flex;
    justify-content: space-between;
    width: 100%;
    margin-top: 20px;
  }

  .card {
    background-color: #fff;
    border-radius: 10px;
    padding: 1rem;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    flex: 1;
    margin: 0 0.5rem;
    position: relative;
    height: 300px;
  }

  .card-content {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: center;
    position: relative;
    padding: 1rem;
  }

  .top-center-icon {
    color: white !important;
    font-size: 2.0rem !important;
    margin-bottom: 1rem;
  }

  .number {
    font-size: 5rem;
    font-weight: bold;
    margin-bottom: 1rem;
  }

  .bottom-center-text {
    font-size: 1.0rem;
  }

  .bottom-reserved-space {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 3rem; /* Adjust height as needed */
  }

  .navigate-button {
    background-color: transparent;
    border: 2px solid white;
    color: black;
    border-radius: 20px;
    padding: 0 16px;
    font-weight: bold;
    min-width: auto;
  }

    .navigate-button v-icon {
      margin-left: 8px;
    }

  .average-time-card,
  .total-tickets-card,
  .success-rate-card {
    background-color: #003399;
    color: white;
  }

  .breached-card {
    background-color: #6d6d6d;
    color: white;
  }
</style>
