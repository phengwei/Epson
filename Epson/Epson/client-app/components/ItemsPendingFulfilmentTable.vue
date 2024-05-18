<template>
  <v-data-table :headers="headers"
                :items="filteredItemsPendingFulfilment"
                :loading="loading"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>New Request</v-toolbar-title>
        <v-divider class="mx-4" inset vertical></v-divider>
        <v-text-field v-model="search"
                      class="search-input"
                      append-icon="mdi-magnify"
                      label="Search by end user or request #"
                      single-line
                      hide-details></v-text-field>
        <v-spacer></v-spacer>
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
        search: '',
        loading: true,
        RequestProductStatusEnum
      }
    },
    computed: {
      filteredItemsPendingFulfilment() {
        if (this.search.trim() === '') {
          return this.itemsPendingFulfilment;
        }
        return this.itemsPendingFulfilment.filter(request => {
          const requestIdMatch = String(request.requestId).toLowerCase().includes(this.search.toLowerCase());
          const projectNameMatch = request.projectName && request.projectName.toLowerCase().includes(this.search.toLowerCase());
          return requestIdMatch || projectNameMatch;
        });
      },
    },
    watch: {
      options: {
        handler() {
          this.getFulfillerItem()
        },
        deep: true,
      }
    },
    created() {
      this.getFulfillerItem();
    },
    methods: {
      viewRequest(request) {
        // Filter out the request products based on the selected product's ID and price
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
      getFulfillerItem() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfilleritem`).then(result => {
          this.itemsPendingFulfilment = [];
          result.data.data.forEach(item => {
            item.requestProductsModel.forEach(product => {
              if (product.authorizedToFulfill && product.status === RequestProductStatusEnum.Pending) {
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
                    price: comp.price
                  }
                  newItem.competitors.push(c);
                });
                this.itemsPendingFulfilment.push(newItem);
              }
            });
          });
          this.loading = false;
        })
      },
    },
  }
</script>

<style>
  .search-input {
    flex-grow: 1;
    margin-left: 16px;
    margin-right: 16px;
    width: 5%;
  }
</style>
