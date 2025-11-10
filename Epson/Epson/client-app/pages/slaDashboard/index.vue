<template>
  <v-app>
    <div class="page">
      <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
        <v-card-title>
          <v-toolbar flat>
            <v-toolbar-title>
              <h2 class="blue-text big-bold">SLA OVERVIEW</h2>
            </v-toolbar-title>
          </v-toolbar>
        </v-card-title>

        <v-card-text>
          <div class="filter-bar">
            <label for="month-select" class="filter-label">Select Month:</label>
            <v-select v-model="selectedMonth"
                      :items="months"
                      item-text="text"
                      item-value="value"
                      outlined
                      dense
                      class="month-select" />
            <label for="year-select" class="filter-label year-label">Year:</label>
            <v-select v-model="selectedYear"
                      :items="years"
                      item-text="text"
                      item-value="value"
                      outlined
                      dense
                      class="year-select" />
          </div>

          <div class="container">
            <!-- Total Open Tickets -->
            <div class="card total-open-card">
              <div class="card-content">
                <v-icon class="top-center-icon">mdi-ticket-confirmation-outline</v-icon>
                <h2 class="number">{{ TotalOpenTickets }}</h2>
                <p class="bottom-center-text">Total Open Tickets</p>
                <div class="bottom-reserved-space"></div>
              </div>
            </div>

            <!-- Total Closed Tickets -->
            <div class="card total-closed-card">
              <div class="card-content">
                <v-icon class="top-center-icon">mdi-ticket-outline</v-icon>
                <h2 class="number">{{ TotalClosedTickets }}</h2>
                <p class="bottom-center-text">Total Closed Tickets</p>
                <div class="bottom-reserved-space"></div>
              </div>
            </div>

            <!-- Breached Tickets -->
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

            <!-- Success Rate -->
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
      const thisYear = new Date().getFullYear();
      const years = [{ value: 0, text: 'All' }];
      for (let y = thisYear - 3; y <= thisYear + 3; y++) {
        years.push({ value: y, text: String(y) });
      }
      return {
        // removed AverageTimeToResolutionInHours
        BreachedTickets: 0,
        TotalOpenTickets: 0,
        TotalClosedTickets: 0,
        SuccessRate: 0,
        selectedMonth: new Date().getMonth() + 1,
        selectedYear: thisYear,
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
        years,
      };
    },
    watch: {
      selectedMonth(newMonth) {
        this.getSLAMetrics(newMonth);
      },
      selectedYear(newYear) {
        this.getSLAMetrics(this.selectedMonth, newYear);
      },
    },
    mounted() {
      this.getSLAMetrics();
      this.modifySelectInputs();
    },
    methods: {
      modifySelectInputs() {
        const vSelects = this.$el.querySelectorAll('.v-select');
        vSelects.forEach(vSelect => {
          const inputElement = vSelect.querySelector('input[type="text"]');
          if (inputElement) {
            inputElement.removeAttribute('type');
          }
        });
      },
      async getSLAMetrics(month = this.selectedMonth, year = this.selectedYear) {
        try {
          const result = await this.$axios.get(`${this.$config.restUrl}/api/sla/getslametrics`, {
            params: { month, year },
          });

          if (result.data && result.data.data) {
            const d = result.data.data;
            console.log("dd", d);
            this.TotalOpenTickets = d.totalOpenTickets ?? 0;
            this.TotalClosedTickets = d.totalCloseTickets ?? 0;
            this.BreachedTickets = d.breachedTickets ?? d.BreachedTickets ?? 0;
            this.SuccessRate = d.successRate ?? d.SuccessRate ?? 0;
          } else {
            this.resetMetrics();
          }
        } catch (error) {
          console.error('There was a problem fetching the SLA metrics:', error);
          this.resetMetrics();
        }
      },
      resetMetrics() {
        this.BreachedTickets = 0;
        this.TotalOpenTickets = 0;
        this.TotalClosedTickets = 0;
        this.SuccessRate = 0;
        console.log('Metrics reset.');
      },
      goToBreachedTickets() {
        this.$router.push({
          path: '/request',
          query: { breached: true, month: this.selectedMonth, year: this.selectedYear },
        });
      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';

  .v-application {
    font-family: TCCC-UnityText-Regular, TCCC-UnityText !important;
  }

  .blue-text {
    color: #003399;
    font-weight: bold;
  }

  .theme--light.v-btn.v-btn--has-bg {
    background-color: #f5f5f5 !important;
    color: black !important;
  }

  .big-bold {
    font-size: 1.5rem;
  }

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
    align-items: center;
    gap: 8px;
    justify-content: center;
    width: 45%;
  }

    .filter-bar label {
      margin-right: 0.5rem;
      font-weight: bold;
    }

  .month-select,
  .year-select {
    width: 100%;
    max-width: 180px;
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
    height: 3rem;
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

  /* Blue theme cards */
  .total-open-card,
  .total-closed-card,
  .success-rate-card {
    background-color: #003399;
    color: white;
  }

  /* Grey theme card */
  .breached-card {
    background-color: #6d6d6d;
    color: white;
  }
</style>
