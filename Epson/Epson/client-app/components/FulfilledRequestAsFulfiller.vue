<template>
  <v-data-table :headers="headers"
                :items="flattenedRequests"
                :items-per-page="5"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>Fulfilled Request</v-toolbar-title>
        <v-divider class="mx-4" inset vertical></v-divider>
        <v-spacer></v-spacer>
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
          { text: 'Product Name', value: 'productName' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Budget', value: 'budget' },
          { text: 'Fulfilled Price', value: 'fulfilledPrice' },
          { text: 'Fulfilled Date', value: 'fulfilledDate' },
          { text: 'Actions', value: 'action', sortable: false }
        ],
        requests: [],
      };
    },
    computed: {
      flattenedRequests() {
        return this.requests.map(product => ({
          id: product.requestId,
          requestedBy: product.requestedBy,
          productId: product.productId,
          productName: product.productName,
          quantity: product.quantity,
          budget: product.endUserPrice,
          fulfilledPrice: product.dealerPrice,
          fulfilledDate: moment(product.fulfilledDate).format('MMMM Do YYYY')
        }));
      }
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
            createdOnUTC: moment(item.createdOnUTC).format('MMMM Do YYYY')
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
  .vh-100 {
    height: 100vh;
  }
</style>
