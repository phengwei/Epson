<template>
  <v-data-table :headers="headers"
                :items="filteredFlattenedRequests"
                :items-per-page="10"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title class="blue-text big-bold">FULFILLED REQUEST</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="search"
                      prepend-inner-icon="mdi-magnify"
                      placeholder="Search by end user or request #"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"></v-text-field>
      </v-toolbar>
    </template>
    <template v-slot:item.action="{ item }">
      <v-btn @click="viewRequest(item)">View</v-btn>
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
        search: '',
        requests: [],
      };
    },
    computed: {
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
    methods: {
      async getRequest(requestId) {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/request/getrequests`);

          const requests = response.data.data.map(item => ({
            ...item,
            createdOnUTC: moment(item.createdOnUTC).format('DD MMM YY HH:mm')
          }));

          const request = requests.find(req => req.id === requestId);
          return request;

        } catch (error) {
          console.error('Error fetching requests:', error);
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


      getFulfilledRequestAsFulfiller() {
        this.$axios.get(`${this.$config.restUrl}/api/request/getfulfilledrequestasfulfiller`)
          .then(response => {
            this.requests = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching fulfilled request:', error);
          });
      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
