<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto" style="width: 90%">
      <v-card-title>
        Product Audit Trail
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="auditTrails"
                      :items-per-page="5"
                      :options.sync="options"
                      :loading="loading"
                      class="elevation-1">
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import moment from 'moment';

  export default {
    name: 'ProductAuditTrail',
    data() {
      return {
        headers: [
          { text: 'Product ID', value: 'id' },
          { text: 'Action', value: 'action' },
          { text: 'Action Details', value: 'actionDetails' },
          { text: 'Date', value: 'actionTime' }
        ],
        auditTrails: [],
        options: {},
        loading: false,
      };
    },
    created() {
      this.getProductAuditTrail();
    },
    methods: {
      getProductAuditTrail() {
        this.$axios.get(`${this.$config.restUrl}/api/audittrail/getproductaudittrail`)
          .then(response => {
            this.auditTrails = response.data.data.map(item => {
              item.actionTime = this.formatDate(item.actionTime);
              return item;
            });
            this.loading = false;
          })
          .catch(error => {
            console.error('Error fetching product audit trail:', error);
            this.loading = false;
          });
      },
      formatDate(dateString) {
        return moment(dateString).format('MMM DD, YYYY, h:mm:ss a');
      }
    },
  };
</script>

<style scoped>
  .vh-100 {
    height: 100vh;
  }
</style>
