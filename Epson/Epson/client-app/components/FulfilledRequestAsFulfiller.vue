<template>
  <v-data-table :headers="headers"
                :items="this.fulfilledItems"
                :loading="loading"
                :server-items-length="totalItems"
                :items-per-page="paginationOptions.itemsPerPage"
                :options.sync="paginationOptions"
                :footer-props="{ 'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }] }"
                @update:options="updateOptions"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title class="blue-text big-bold">FULFILLED REQUEST</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="searchTerm"
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
        <td>{{ item.id }}</td>
        <td>{{ item.requestedBy }}</td>
        <td>{{ item.categories }}</td>
        <td>{{ item.projectName }}</td>
        <td>{{ item.lastFulfilledDate }}</td>
        <td>{{ item.overallRequestStatusStr }}</td>
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

  export default {
    name: 'FulfilledRequestAsFulfiller',
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'id' },
          { text: 'Requester', value: 'requestedBy' },
          { text: 'Categories', value: 'categories' },
          { text: 'End User', value: 'projectName' },
          { text: 'Last Fulfilled', value: 'lastFulfilledDate' },
          { text: 'Request Status', value: 'overallRequestStatusStr' },
          { text: 'Actions', value: 'action', sortable: false }
        ],
        fulfilledItems: [],
        searchTerm: '',
        search: '',
        requests: [],
        totalItems: 0,
        loading: true,
        paginationOptions: {
          page: 1,
          itemsPerPage: 10,
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
      flattenedRequests() {
        return this.requests.map(product => ({
          projectInformationModel: product.projectInformationModel,
          id: product.requestId,
          requestedBy: product.requestedBy,
          productId: product.productId,
          productName: product.productName,
          quantity: product.quantity,
          budget: product.endUserPrice,
          overallRequestStatusStr: product.overallRequestStatusStr,
          fulfilledPrice: product.dealerPrice,
          fulfilledDate: moment(product.fulfilledDate).format('DD MMM YY HH:mm'),
          projectName: product.projectName
        }));
      },
      filteredFlattenedRequests() {
        if (this.search.trim() === '') {
          return this.flattenedRequests;
        }
        return this.flattenedRequests.filter(request => {
          const requestIdMatch = String(request.id).toLowerCase().includes(this.search.toLowerCase());
          const projectNameMatch = request.projectName &&
            request.projectName.toLowerCase().includes(this.search.toLowerCase());
          return requestIdMatch || projectNameMatch;
        });
      },
    },
    created() {
      this.getFulfilledRequestAsFulfiller();
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
      async getRequest(requestId) {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/request/getrequestbyid`, {
            params: { id: requestId }
          });

          const request = response.data.data;
          request.createdOnUTC = moment(request.createdOnUTC).format('DD MMM YY HH:mm');

          return request;
        } catch (error) {
          console.error('Error fetching request:', error);
        }
      },

      viewRequest(req) {
        const queryParameters = { view: true, requestId: req.id };

        const encodedParams = Base64.encode(JSON.stringify(queryParameters));

        this.$router.push({
          path: '/createquotation',
          query: { params: encodedParams }
        });
      },

      triggerSearch() {
        this.search = this.searchTerm;
        this.paginationOptions.page = 1;
        this.getFulfilledRequestAsFulfiller();
      },
      updateOptions(options) {
        this.paginationOptions = options;
        this.page = options.page;
        this.itemsPerPage = options.itemsPerPage;
        this.getFulfilledRequestAsFulfiller();
      },
      getFulfilledRequestAsFulfiller() {
        this.loading = true;
        const params = {
          search: this.search,
          page: this.paginationOptions.page,
          itemsPerPage: this.paginationOptions.itemsPerPage,
        };
        this.$axios.get(`${this.$config.restUrl}/api/request/getfulfilledrequestasfulfiller`, { params })
          .then(response => {
            this.fulfilledItems = [];
            response.data.data.forEach(item => {
              const lastFulfilledDate = item.requestProducts.reduce((latest, product) => {
                return latest > new Date(product.fulfilledDate) ? latest : new Date(product.fulfilledDate);
              }, new Date(0));

              let overallRequestStatusString;
              if (item.approvalState > 50 && item.approvalState !== 120) {
                overallRequestStatusString = 'Failed';
              } else if (item.approvalState === 50) {
                overallRequestStatusString = 'Approved';
              } else {
                overallRequestStatusString = 'Pending';
              }

              const newItem = {
                id: item.id,
                requestedBy: item.createdByStr,
                projectName: item.projectInformation.projectName,
                lastFulfilledDate: moment(lastFulfilledDate).add(8, 'hours').format('DD MMM YY HH:mm'),
                overallRequestStatusStr: overallRequestStatusString,
                categories: item.categories
              };

              this.fulfilledItems.push(newItem);
            });
            this.totalItems = response.data.count;
            this.loading = false;
          })
          .catch(error => {
            console.error('Error fetching fulfilled request:', error);
            this.loading = false;
          });

      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
