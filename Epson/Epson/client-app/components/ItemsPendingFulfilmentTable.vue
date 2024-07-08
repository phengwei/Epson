<template>
  <v-data-table :headers="headers"
                :items="filteredItemsPendingFulfilment"
                :loading="loading"
                :server-items-length="totalItems"
                :items-per-page="paginationOptions.itemsPerPage"
                :options.sync="paginationOptions"
                :footer-props="{ 'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }] }"
                @update:options="updateOptions"
                class="elevation-1"
                disable-sort>
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title class="blue-text big-bold">NEW REQUEST</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="search"
                      append-icon="mdi-magnify"
                      placeholder="Search by end user or request #"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"
                      @keyup.enter="triggerSearch"
                      @click:append="triggerSearch"></v-text-field>
      </v-toolbar>
    </template>

    <template v-slot:item="{ item }">
      <tr>
        <td>{{ item.requestId }}</td>
        <td>{{ item.createdOnUTC }}</td>
        <td>{{ item.projectName }}</td>
        <td>{{ item.createdByStr }}</td>
        <td>{{ item.productName }}</td>
        <td>{{ item.endUserPrice }}</td>
        <td>{{ item.quantity }}</td>
        <td>
          <v-btn @click="viewRequest(item)">View</v-btn>
        </td>
      </tr>
    </template>
  </v-data-table>
</template>

<script>
  import moment from 'moment';
  import { RequestProductStatusEnum } from '~/script/requestProductStatusEnum.js';

  export default {
    name: 'ItemsPendingFulfilmentTable',
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'requestId' },
          { text: 'Requested On', value: 'createdOnUTC' },
          { text: 'End User', align: 'start', value: 'projectName' },
          { text: 'Requested By', value: 'createdByStr' },
          { text: 'Product', value: 'productName' },
          { text: 'Budget', value: 'endUserPrice' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Fulfill Request', value: 'actions', sortable: false }
        ],
        itemsPendingFulfilment: [],
        totalItems: 0,
        search: '',
        loading: true,
        RequestProductStatusEnum,
        paginationOptions: {
          page: 1,
          itemsPerPage: 10,
          sortBy: [],
          sortDesc: []
        }
      };
    },
    computed: {
      query() {
        return {
          page: this.paginationOptions.page,
          itemsPerPage: this.paginationOptions.itemsPerPage,
          search: this.search
        };
      },
      filteredItemsPendingFulfilment() {
        return this.itemsPendingFulfilment;
      }
    },
    created() {
      this.getFulfillerItem();
    },
    mounted() {
      this.modifySelectInputs();
    },
    methods: {
      modifySelectInputs() {
        const vSelects = this.$el.querySelectorAll('.v-text-field__slot');
        vSelects.forEach(vSelect => {
          const inputElement = vSelect.querySelector('input[type="text"]');
          if (inputElement) {
            inputElement.removeAttribute('type');
          }
        });
      },
      triggerSearch() {
        this.paginationOptions.page = 1;
        this.getFulfillerItem();
      },
      viewRequest(request) {
        console.log("awd", request);
        const selectedProduct = request;
        const filteredRequest = {
          ...request,
          requestProductsModel: [selectedProduct]
        };

        let queryParameters = { view: true, request: JSON.stringify(filteredRequest) };

        if (selectedProduct.isCoverplus === true) {
          queryParameters = { ...queryParameters, isFulfillCoverplus: true };
        } else if (selectedProduct.isCoverplus === false) {
          queryParameters = { ...queryParameters, isFulfill: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
        });
      },
      updateOptions(options) {
        this.paginationOptions = options;
        this.getFulfillerItem();
      },
      getFulfillerItem() {
        this.loading = true;
        const params = {
          search: this.search,
          page: this.paginationOptions.page,
          itemsPerPage: this.paginationOptions.itemsPerPage
        };
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfilleritem`, { params }).then(result => {
          this.itemsPendingFulfilment = [];
          console.log("awd", result);
          result.data.data.items.forEach(item => {
            item.requestProducts.forEach(product => {
              if (product.authorizedToFulfill && product.status === this.RequestProductStatusEnum.Pending) {
                const newItem = {
                  ...item,
                  ...product,
                  projectName: item.projectInformation && item.projectInformation.projectName ? item.projectInformation.projectName : 'N/A',
                  createdOnUTC: moment(product.createdOnUTC).format('DD MMM YY HH:mm'),
                  productName: product.productName,
                  distyPrice: product.distyPrice,
                  dealerPrice: product.dealerPrice,
                  endUserPrice: product.endUserPrice,
                  quantity: product.quantity,
                  authorizedToFulfill: product.authorizedToFulfill,
                  competitors: [],
                  status: product.status
                };

                item.competitorInformations.forEach(comp => {
                  const c = {
                    model: comp.model,
                    brand: comp.brand,
                    price: comp.price
                  };
                  newItem.competitors.push(c);
                });
                this.itemsPendingFulfilment.push(newItem);
              }
            });
          });
          this.totalItems = result.data.data.total;
          this.loading = false;
        });
      }
    }
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';

  v-btn {
    background-color: #003399 !important;
  }

  .filter-container {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .month-selector {
    width: 20%;
    min-width: 150px;
  }

  .create-quotation {
    margin-left: 16px;
  }

  .vh-100 {
    height: 100vh;
  }

  .form-group {
    margin-bottom: 1rem;
    display: flex;
    justify-content: center;
    flex-direction: column;
  }

  label {
    font-weight: bold;
    margin-bottom: 0.5rem;
    color: black;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 10px;
    padding: 0.5rem;
    width: 100%;
  }

  .role-checkbox {
    display: flex;
    align-items: center;
    margin-bottom: 0.5rem;
  }

    .role-checkbox input[type="checkbox"] {
      margin-right: 0.5rem;
    }

      .role-checkbox input[type="checkbox"].styled-checkbox {
        appearance: none;
        width: 16px;
        height: 16px;
        border: 1px solid #003399;
        border-radius: 4px;
        position: relative;
        cursor: pointer;
      }

        .role-checkbox input[type="checkbox"].styled-checkbox:checked::before {
          font-size: 12px;
          color: #003399;
          position: absolute;
          top: 1px;
          left: 2px;
        }

  .search-bar {
    width: 150px;
    border-radius: 20px;
    padding: 5px 10px;
    border-width: medium;
  }

    .search-bar .v-input__control {
      background-color: #003399;
    }

  .theme--light.v-btn.v-btn--has-bg {
    background-color: #003399 !important;
    color: white !important;
  }

  .search-bar .v-field__append-inner,
  .search-bar .v-field__prepend-inner {
    margin-top: 0;
  }

  .search-bar .v-input__control {
    border: none;
  }

  .blue-button {
    background-color: #003399 !important;
    color: white !important;
  }

  .blue-text {
    color: #003399 !important;
  }

  .blue-text--active {
    color: #003399 !important;
  }

  .big-bold {
    font-size: 24px;
    font-weight: bold;
  }

  .small-bold {
    font-size: 16px;
    font-weight: bold;
  }

  .card-round {
    border-radius: 20px !important;
  }

  .table-padding {
    padding: 20px;
  }
</style>
