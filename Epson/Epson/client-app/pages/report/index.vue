<template>
  <v-app>
    <v-main>
      <div class="report-container">
        <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
          <v-toolbar flat>
            <v-toolbar-title><h1 class="blue-text big-bold">Report</h1></v-toolbar-title>
          </v-toolbar>
          <v-card-text>
            <div class="filter-container mb-4">
              <span class="blue-text small-bold">Requester Performance</span>
            </div>
            <v-radio-group v-model="selectedOption">
              <v-row class="compact-row">
                <v-col cols="3">
                  <v-radio label="Requester" value="requester"></v-radio>
                </v-col>
                <v-col cols="3">
                  <v-select v-model="requester" :items="requesters" item-text="text" item-value="value" dense outlined></v-select>
                </v-col>
              </v-row>
              <v-row class="compact-row">
                <v-col cols="3">
                  <v-radio label="All Requesters in" value="all_requesters_in"></v-radio>
                </v-col>
                <v-col cols="3">
                  <v-select v-model="month" :items="months" item-text="text" item-value="value" label="All Requesters in" dense outlined></v-select>
                </v-col>
              </v-row>
              <v-row class="compact-row">
                <v-col cols="3">
                  <v-radio label="All Requesters from" value="all_requesters_from"></v-radio>
                </v-col>
                <v-col cols="3">
                  <v-select v-model="fromMonth" :items="months" item-text="text" item-value="value" label="From" dense outlined></v-select>
                </v-col>
                <v-col cols="3">
                  <v-select v-model="toMonth" :items="months" item-text="text" item-value="value" label="To" dense outlined></v-select>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="3">
                  <v-btn class="blue-button" color="primary" @click="fetchReport">
                    CONFIRM
                  </v-btn>
                </v-col>
              </v-row>
            </v-radio-group>
          </v-card-text>
          <div class="donut-chart-container mt-4">
            <donut-chart :chart-data="donutChartData" :options="donutOptions" ref="donutChart"></donut-chart>
          </div>
          <div class="bar-chart-container">
            <bar-chart :chart-data="combinedChartData" :options="options" ref="barChart"></bar-chart>
          </div>
        </v-card>
      </div>
    </v-main>
  </v-app>
</template>

<script>
  import DonutChart from '~/components/DonutChart.vue';
  import BarChart from '~/components/BarChart.vue';

  export default {
    name: 'ReportingDashboard',
    data() {
      return {
        selectedOption: 'requester',
        requester: null,
        month: null,
        fromMonth: null,
        toMonth: null,
        combinedChartData: null,
        donutChartData: {
          labels: [],
          datasets: [
            {
              data: [],
              backgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF'],
              hoverBackgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF']
            }
          ]
        },
        topproductsbyrevenue: null,
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            xAxes: [{
              type: 'time',
              time: {
                unit: 'month',
                tooltipFormat: 'MMM YYYY',
                displayFormats: {
                  month: 'MMM YYYY'
                }
              },
              gridLines: {
                display: true
              },
              ticks: {
                source: 'auto'
              }
            }],
            yAxes: [{
              ticks: {
                beginAtZero: true,
                callback: value => Math.floor(value)
              },
              gridLines: {
                display: true
              }
            }]
          },
          tooltips: {
            callbacks: {
              label: tooltipItem => 'Request Count: ' + tooltipItem.yLabel
            }
          },
          legend: {
            display: true,
            position: 'top',
            labels: {
              boxWidth: 20
            }
          }
        },
        donutOptions: {
          responsive: true,
          maintainAspectRatio: false,
          legend: {
            display: true,
            position: 'right',
            labels: {
              generateLabels: chart => {
                const data = chart.data;
                if (!data || !data.labels || !data.datasets[0] || !data.datasets[0].data) return [];
                return data.labels.map((label, i) => {
                  const value = data.datasets[0].data[i];
                  return {
                    text: `${label}: ${value}`,
                    fillStyle: data.datasets[0].backgroundColor[i],
                    hidden: isNaN(value) || value <= 0,
                    lineCap: data.datasets[0].borderCapStyle,
                    lineDash: data.datasets[0].borderDash,
                    lineDashOffset: data.datasets[0].borderDashOffset,
                    lineJoin: data.datasets[0].borderJoinStyle,
                    strokeStyle: data.datasets[0].borderColor ? data.datasets[0].borderColor[i] : 'rgba(0,0,0,0)',
                    pointStyle: data.datasets[0].pointStyle ? data.datasets[0].pointStyle[i] : 'circle',
                    datasetIndex: 0
                  };
                });
              }
            }
          },
          plugins: {
            datalabels: {
              formatter: (value, context) => {
                const total = context.chart.data.datasets[0].data.reduce((acc, val) => acc + val, 0);
                const percentage = Math.round((value / total) * 100);
                return `${percentage}%`;
              },
              color: '#fff',
              font: {
                weight: 'bold'
              }
            }
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
      };
    },
    async created() {
      await this.fetchRequesters();
    },
    methods: {
      async fetchRequesters() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/customer/getallrequesters`);
          const rawData = response.data.data;
          const formattedRequesters = rawData.map(item => ({
            text: item.userName,
            value: item.id
          }));
          this.requesters = formattedRequesters;
        } catch (error) {
          console.error('Error fetching requesters:', error);
        }
      },
      async fetchReport() {
        let selectedValues = {};

        if (this.selectedOption === 'requester') {
          selectedValues = {
            requesterId: this.requester,
            month: 0,
            allRequester: false
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyrequester`, { params: selectedValues });
            const data = response.data.data;

            // Ensure the date is correctly parsed
            const labels = data.map(item => new Date(item.date).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }));
            const values = data.map(item => item.totalNumberOfSales);
            this.combinedChartData = {
              labels,
              datasets: [
                {
                  label: 'Monthly Request Count',
                  backgroundColor: 'transparent',
                  borderColor: '#003399',
                  pointBackgroundColor: '##003399',
                  pointBorderColor: '#fff',
                  pointHoverBackgroundColor: '#fff',
                  pointHoverBorderColor: '#003399',
                  data: values,
                  fill: false,
                  borderWidth: 1
                }
              ]
            };
          } catch (error) {
            console.error('Error fetching report:', error);
          }
        } else if (this.selectedOption === 'all_requesters_in') {
          selectedValues = {
            fromMonth: this.month,
            toMonth: this.month,
            allRequester: true
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyrequesterbydonut`, { params: selectedValues });
            const data = response.data.data;
            const labels = data.map(item => item.requesterName);
            const values = data.map(item => item.totalNumberOfSales);
            this.donutChartData = {
              labels,
              datasets: [
                {
                  data: values,
                  backgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF'],
                  hoverBackgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF']
                }
              ]
            };
          } catch (error) {
            console.error('Error fetching report:', error);
          }
        } else if (this.selectedOption === 'all_requesters_from') {
          selectedValues = {
            fromMonth: this.fromMonth,
            toMonth: this.toMonth,
            allRequester: true
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyrequesterbydonut`, { params: selectedValues });
            const data = response.data.data;
            const labels = data.map(item => item.requesterName);
            const values = data.map(item => item.totalNumberOfSales);
            this.donutChartData = {
              labels,
              datasets: [
                {
                  data: values,
                  backgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF'],
                  hoverBackgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF']
                }
              ]
            };
          } catch (error) {
            console.error('Error fetching report:', error);
          }
        }
      }
    },
    components: {
      BarChart,
      DonutChart
    }
  };
</script>

<style>
  .report-container {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .filter-container {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .filter-options {
    margin-top: 20px;
  }

  .chart-container {
    margin-top: 20px;
  }

  .blue-text {
    color: #003399;
    font-weight: bold;
  }

  .big-bold {
    font-size: 1.5rem;
  }

  .small-bold {
    font-size: 1.2rem;
    font-weight: bold;
  }

  .blue-button {
    background-color: #003399;
    color: white;
  }

  .card {
    width: 100%;
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

  .compact-row {
    margin-bottom: 8px;
  }

  .v-select {
    max-width: 100%;
    border-radius: 10px;
    padding: 0 10px;
    box-shadow: none; /* Remove box shadow */
  }

    .v-select .v-input__control {
      padding: 5px !important;
      background-color: #fff !important;
    }

    .v-select .v-input__slot {
      padding: 5px 10px !important;
    }

    .v-select .v-input__control .v-input__slot {
      border: none;
      box-shadow: none;
    }

      .v-select .v-input__control .v-input__slot:focus-within {
        border: none;
        box-shadow: none;
      }

  .bar-chart-container {
    height: 400px;
    width: 100%;
  }

  .donut-chart-container {
    height: 400px;
    width: 100%;
  }

  @media (max-width: 768px) {
    .chart-container {
      width: 100%;
    }
  }
</style>
