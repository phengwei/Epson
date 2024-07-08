<template>
  <v-app>
    <v-main>
      <div class="report-container">
        <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
          <v-card-title>
            <v-toolbar flat>
              <v-toolbar-title>
                <h2 class="blue-text big-bold">REPORTS</h2>
              </v-toolbar-title>
            </v-toolbar>
          </v-card-title>

          <v-card-text>
            <div class="filter-container mb-4">
              <v-menu offset-y>
                <template v-slot:activator="{ on, attrs }">
                  <v-btn color="primary" class="blue-background" dark v-bind="attrs" v-on="on">
                    {{ selectedTab === 'requester' ? 'Requester Performance' : 'Product Performance' }}
                    <v-icon right>mdi-menu-down</v-icon>
                  </v-btn>
                </template>
                <v-list>
                  <v-list-item @click="selectedTab = 'requester'">
                    <v-list-item-title>Requester Performance</v-list-item-title>
                  </v-list-item>
                  <v-list-item @click="selectedTab = 'product'">
                    <v-list-item-title>Product Performance</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </div>
            <v-card-text v-if="selectedTab === 'requester'">
              <v-radio-group v-model="selectedOption">
                <v-row class="compact-row">
                  <v-col cols="3">
                    <v-radio label="Requester" value="requester"></v-radio>
                  </v-col>
                  <v-col cols="3">
                    <v-select v-model="requester" :items="requesters" item-text="text" label="Requester" item-value="value" dense outlined></v-select>
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
              <div v-if="showCharts && selectedOption === 'requester'" class="bar-chart-container">
                <bar-chart :chart-data="combinedChartData" :options="options" ref="barChart"></bar-chart>
              </div>
              <div v-if="showCharts && (selectedOption === 'all_requesters_in' || selectedOption === 'all_requesters_from')" class="donut-chart-container mt-4">
                <donut-chart :chart-data="donutChartData" :options="donutOptions" ref="donutChart"></donut-chart>
              </div>
            </v-card-text>
            <v-card-text v-else>
              <v-radio-group v-model="selectedOption">
                <v-row class="compact-row">
                  <v-col cols="3">
                    <v-radio label="Product" value="product"></v-radio>
                  </v-col>
                  <v-col cols="3">
                    <v-select v-model="product" :items="products" item-text="text" label="Product" item-value="value" dense outlined></v-select>
                  </v-col>
                </v-row>
                <v-row class="compact-row">
                  <v-col cols="3">
                    <v-radio label="All Products in" value="all_products_in"></v-radio>
                  </v-col>
                  <v-col cols="3">
                    <v-select v-model="productMonth" :items="months" item-text="text" item-value="value" label="All Products in" dense outlined></v-select>
                  </v-col>
                </v-row>
                <v-row class="compact-row">
                  <v-col cols="3">
                    <v-radio label="All Products from" value="all_products_from"></v-radio>
                  </v-col>
                  <v-col cols="3">
                    <v-select v-model="productFromMonth" :items="months" item-text="text" item-value="value" label="From" dense outlined></v-select>
                  </v-col>
                  <v-col cols="3">
                    <v-select v-model="productToMonth" :items="months" item-text="text" item-value="value" label="To" dense outlined></v-select>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="3">
                    <v-btn class="blue-button" color="primary" @click="fetchProductReport">
                      CONFIRM
                    </v-btn>
                  </v-col>
                </v-row>
              </v-radio-group>
              <div v-if="showProductCharts && selectedOption === 'product'" class="bar-chart-container">
                <bar-chart :chart-data="productChartData" :options="options" ref="barChart"></bar-chart>
              </div>
              <div v-if="showProductCharts && (selectedOption === 'all_products_in' || selectedOption === 'all_products_from')" class="donut-chart-container mt-4">
                <donut-chart :chart-data="productDonutChartData" :options="donutOptions" ref="donutChart"></donut-chart>
              </div>
            </v-card-text>
          </v-card-text>
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
        selectedTab: 'requester',
        selectedOption: 'requester',
        requester: null,
        month: null,
        fromMonth: null,
        toMonth: null,
        product: null,
        productMonth: null,
        productFromMonth: null,
        productToMonth: null,
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
        productChartData: null,
        productDonutChartData: {
          labels: [],
          datasets: [
            {
              data: [],
              backgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF'],
              hoverBackgroundColor: ['#003399', '#0044CC', '#0066FF', '#0088FF', '#00AAFF', '#00CCFF', '#00EEFF', '#00FFFF', '#33FFFF', '#66FFFF']
            }
          ]
        },
        showCharts: false,
        showProductCharts: false,
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
                beginAtZero: true
              },
              gridLines: {
                display: true
              }
            }]
          },
          tooltips: {
            callbacks: {
              label: (tooltipItem) => {
                return 'Request Count: ' + tooltipItem.yLabel;
              }
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
        months: this.generateMonths().reverse()
      };
    },
    async created() {
      await this.fetchRequesters();
      await this.fetchproducts();
    },
    mounted() {
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
      generateMonths() {
        const months = [];
        const currentYear = new Date().getFullYear();
        const currentMonth = new Date().getMonth() + 1; 

        for (let year = currentYear - 1; year <= currentYear; year++) {
          for (let month = 1; month <= 12; month++) {
            if (year === currentYear && month > currentMonth) {
              break;
            }
            const monthName = new Date(year, month - 1).toLocaleString('en-US', { month: 'long' });
            months.push({ text: `${monthName} ${year}`, value: new Date(year, month - 1, 1).toISOString() });
          }
        }

        return months;
      },
      getStartOfMonth(dateString) {
        const date = new Date(dateString);
        return new Date(date.getFullYear(), date.getMonth(), 1).toISOString();
      },
      getEndOfMonth(dateString) {
        const date = new Date(dateString);
        return new Date(date.getFullYear(), date.getMonth() + 1, 0).toISOString();
      },
      async fetchproducts() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproducts`);
          const rawData = response.data.data;
          const formattedProducts = rawData.map(item => ({
            text: item.name,
            value: item.id
          }));
          this.products = formattedProducts;
        } catch (error) {
          console.error('Error fetching requesters:', error);
        }
      },
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
        this.showCharts = true;
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

            const labels = data.map(item => new Date(item.date).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }));
            const values = data.map(item => item.totalNumberOfSales);
            this.combinedChartData = {
              labels,
              datasets: [
                {
                  label: 'Monthly Request Count',
                  backgroundColor: 'transparent',
                  borderColor: '#003399',
                  pointBackgroundColor: '#003399',
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
            fromMonth: this.getStartOfMonth(this.month),
            toMonth: this.getEndOfMonth(this.month),
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
            fromMonth: this.getStartOfMonth(this.fromMonth),
            toMonth: this.getEndOfMonth(this.toMonth),
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
      },
      async fetchProductReport() {
        this.showProductCharts = true;
        let selectedValues = {};

        if (this.selectedOption === 'product') {
          selectedValues = {
            productId: this.product,
            month: 0
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyproduct`, { params: selectedValues });
            const data = response.data.data;

            const labels = data.map(item => new Date(item.date).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }));
            const values = data.map(item => item.totalNumberOfSales);
            this.productChartData = {
              labels,
              datasets: [
                {
                  label: 'Monthly Product Sales',
                  backgroundColor: 'transparent',
                  borderColor: '#003399',
                  pointBackgroundColor: '#003399',
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
            console.error('Error fetching product report:', error);
          }
        } else if (this.selectedOption === 'all_products_in') {
          selectedValues = {
            fromMonth: this.getStartOfMonth(this.productMonth),
            toMonth: this.getEndOfMonth(this.productMonth),
            allProduct: true
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyproductbydonut`, { params: selectedValues });
            const data = response.data.data;
            const labels = data.map(item => item.productName);
            const values = data.map(item => item.totalNumberOfSales);
            this.productDonutChartData = {
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
            console.error('Error fetching product report:', error);
          }
        } else if (this.selectedOption === 'all_products_from') {
          selectedValues = {
            fromMonth: this.getStartOfMonth(this.productFromMonth),
            toMonth: this.getEndOfMonth(this.productToMonth),
            allProduct: true
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/report/getmonthlysalesbyproductbydonut`, { params: selectedValues });
            const data = response.data.data;
            const labels = data.map(item => item.productName);
            const values = data.map(item => item.totalNumberOfSales);
            this.productDonutChartData = {
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
            console.error('Error fetching product report:', error);
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

<style scoped>
  .v-application {
    font-family: TCCC-UnityText-Regular, TCCC-UnityText !important;
  }

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

  .theme--dark.v-btn.v-btn--has-bg {
    background-color: #003399 !important;
  }
  .big-bold {
    font-size: 1.5rem;
  }

  .small-bold {
    font-size: 1.2rem;
    font-weight: bold;
  }

  .blue-button {
    background-color: #003399 !important;
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
    box-shadow: none; 
  }

    .v-select .v-input__control {
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
