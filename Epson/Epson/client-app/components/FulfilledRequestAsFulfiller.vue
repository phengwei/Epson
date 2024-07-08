<template>
  <v-data-table :headers="headers"
                :items="filteredFlattenedRequests"
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
        <td>{{ item.projectName }}</td>
        <td>{{ item.productName }}</td>
        <td>{{ item.quantity }}</td>
        <td>{{ item.budget }}</td>
        <td>{{ item.fulfilledPrice }}</td>
        <td>{{ item.fulfilledDate }}</td>
        <td>{{ item.overallRequestStatusStr }}</td>
        <td>
          <v-btn @click="viewRequest(item)">View</v-btn>
        </td>
      </tr>
    </template>
  </v-data-table>
</template>

<script>
  import moment from 'moment';

  export default {
    name: 'FulfilledRequestAsFulfiller',
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'id' },
          { text: 'Requester', value: 'requestedBy' },
          { text: 'End User', value: 'projectName' },
          { text: 'Product', value: 'productName' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Budget', value: 'budget' },
          { text: 'Fulfilled Price', value: 'fulfilledPrice' },
          { text: 'Fulfilled Date', value: 'fulfilledDate' },
          { text: 'Request Status', value: 'overallRequestStatusStr' },
          { text: 'Actions', value: 'action', sortable: false }
        ],
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

      async viewRequest(req) {
        const request = await this.getRequest(req.id);

        const queryParameters = { view: true, request: JSON.stringify(request) };

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
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
            this.requests = response.data.data;
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
