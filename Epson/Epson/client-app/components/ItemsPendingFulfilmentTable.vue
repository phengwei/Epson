<template>
  <v-data-table :headers="headers"
                :items="filteredItemsPendingFulfilment"
                :loading="loading"
                :server-items-length="totalItems"
                :items-per-page="paginationOptions.itemsPerPage"
                :options.sync="paginationOptions"
                :footer-props="{ 'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }] }"
                @update:options="updateOptions"
                class="elevation-1">
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
        <td>{{ item.createdBy }}</td>
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
          { text: 'Requested By', value: 'createdBy' },
          { text: 'Product', value: 'productName' },
          { text: 'Budget', value: 'endUserPrice' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Fulfill Request', value: 'actions', sortable: false },
        ],
        itemsPendingFulfilment: [],
        totalItems: 0,
        search: '',
        loading: true,
        itemsPerPage: 10, // Set default items per page to 10
        page: 1,
        RequestProductStatusEnum,
        paginationOptions: {
          page: 1,
          itemsPerPage: 10, // Set default rows per page to 10
          sortBy: [],
          sortDesc: [],
        },
      };
    },
    computed: {
      query() {
        return {
          page: this.paginationOptions.page,
          itemsPerPage: this.paginationOptions.itemsPerPage,
          search: this.search,
        };
      },
      filteredItemsPendingFulfilment() {
        return this.itemsPendingFulfilment;
      },
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
        this.page = 1; // Reset to the first page on new search
        this.getFulfillerItem();
      },
      viewRequest(request) {
        const selectedProduct = request;
        const filteredRequest = {
          ...request,
          requestProductsModel: [selectedProduct],
        };

        let queryParameters = { view: true, request: JSON.stringify(filteredRequest) };

        if (selectedProduct.isCoverplus === true) {
          queryParameters = { ...queryParameters, isFulfillCoverplus: true };
        } else if (selectedProduct.isCoverplus === false) {
          queryParameters = { ...queryParameters, isFulfill: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters,
        });
      },
      updateOptions(options) {
        this.paginationOptions = options;
        this.page = options.page;
        this.itemsPerPage = options.itemsPerPage;
        this.getFulfillerItem(); // Trigger API call on pagination option change
      },
      getFulfillerItem() {
        this.loading = true;
        const params = {
          search: this.search,
          page: this.page,
          itemsPerPage: this.itemsPerPage,
        };
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfilleritem`, { params }).then(result => {
          this.itemsPendingFulfilment = [];
          let totalRequestProducts = 0; // Initialize total request products counter
          result.data.data.items.forEach(item => {
            item.requestProductsModel.forEach(product => {
              if (product.authorizedToFulfill && product.status === this.RequestProductStatusEnum.Pending) {
                totalRequestProducts++; // Increment counter for each request product
                const newItem = {
                  ...item,
                  ...product,
                  projectName: item.projectInformationModel && item.projectInformationModel.projectName ? item.projectInformationModel.projectName : 'N/A',
                  createdOnUTC: moment(product.createdOnUTC).format('DD MMM YY HH:mm'),
                  productName: product.productName,
                  distyPrice: product.distyPrice,
                  dealerPrice: product.dealerPrice,
                  endUserPrice: product.endUserPrice,
                  quantity: product.quantity,
                  authorizedToFulfill: product.authorizedToFulfill,
                  competitors: [],
                  status: product.status,
                };

                item.competitorInformationModel.forEach(comp => {
                  const c = {
                    model: comp.model,
                    brand: comp.brand,
                    price: comp.price,
                  };
                  newItem.competitors.push(c);
                });
                this.itemsPendingFulfilment.push(newItem);
              }
            });
          });
          this.totalItems = totalRequestProducts; // Set total items to the total number of request products
          this.loading = false;
        });
      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
