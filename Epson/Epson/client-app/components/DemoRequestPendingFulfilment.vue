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
        <v-toolbar-title class="blue-text big-bold">NEW DEMO REQUEST</v-toolbar-title>
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
        <td>{{ item.categories }}</td>
        <td>{{ item.projectName }}</td>
        <td>{{ item.createdByStr }}</td>
        <td>
          <v-btn @click="viewRequest(item)">View</v-btn>
        </td>
      </tr>
    </template>
  </v-data-table>
</template>

<script>
  import { Base64 } from 'js-base64';
  import moment from 'moment';
  import { RequestProductStatusEnum } from '~/script/requestProductStatusEnum.js';

  export default {
    name: 'ItemsPendingFulfilmentTable',
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'requestId' },
          { text: 'Requested On', value: 'createdOnUTC' },
          { text: 'Categories', value: 'categories' },
          { text: 'End User', align: 'start', value: 'projectName' },
          { text: 'Requested By', value: 'createdByStr' },
          { text: 'Approve Request', value: 'actions', sortable: false }
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
        let queryParameters = { view: true, requestId: request.requestId };

        queryParameters = { ...queryParameters, isFinalApprove: true };

        const encodedParams = Base64.encode(JSON.stringify(queryParameters));

        this.$router.push({
          path: '/createquotation',
          query: { params: encodedParams }
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

        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingdirectoritem`, { params })
          .then(result => {
            this.itemsPendingFulfilment = [];

            if (result.data && result.data.data) {
              result.data.data.forEach(item => {
                const newItem = {
                  requestId: item.id,
                  projectName: item.projectInformation && item.projectInformation.projectName ? item.projectInformation.projectName : 'N/A',
                  createdOnUTC: moment(item.createdOnUTC).format('DD MMM YY HH:mm'),
                  createdByStr: item.createdByStr,
                  categories: item.categories
                };

                this.itemsPendingFulfilment.push(newItem);
              });
              this.totalItems = result.data.count || this.itemsPendingFulfilment.length;
            } else {
              this.itemsPendingFulfilment = [];
              this.totalItems = 0;
            }

            this.loading = false;
          })
          .catch(error => {
            console.error('Error fetching pending director items:', error);
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
