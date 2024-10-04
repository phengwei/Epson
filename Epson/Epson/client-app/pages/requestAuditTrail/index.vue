<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-toolbar flat>
        <v-toolbar-title><h2 class="blue-text big-bold">REQUEST AUDIT TRAIL</h2></v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="searchTerm"
                      append-icon="mdi-magnify"
                      placeholder="Search by request #"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"
                      @keyup.enter="triggerSearch"
                      @click:append="triggerSearch"></v-text-field>
      </v-toolbar>
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
    name: 'requestRejectionAuditTrail',
    data() {
      return {
        headers: [
          { text: 'ID', value: 'id' },
          { text: 'Action', value: 'action' },
          { text: 'Action Details', value: 'actionDetails' },
          { text: 'Date', value: 'actionTime' }
        ],
        auditTrails: [],
        options: {},
        loading: true,
        search: '',
        searchTerm: '',
      };
    },
    mounted() {
      this.modifySelectInputs();
    },
    created() {
      this.getRequestAuditTrail();
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
        this.search = this.searchTerm;
        this.options.page = 1;
        this.getRequestAuditTrail();
      },
      getRequestAuditTrail() {
        console.log("test");
        const params = {
          search: this.search,
          page: this.options.page,
          itemsPerPage: this.options.itemsPerPage,
        };
        this.$axios.get(`${this.$config.restUrl}/api/audittrail/getrequestaudittrails`, {params})
          .then(response => {
            this.auditTrails = response.data.data.map(item => {
              item.actionTime = this.formatDate(item.actionTime);
              return item;
            });
            this.loading = false;
          })
          .catch(error => {
            console.error('Error fetching rejection audit trail:', error);
            this.loading = false;
          });
      },
      formatDate(dateString) {
        return moment(dateString).add(8, 'hours').format('DD MMM YY HH:mm');
      }
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
  .theme--light.v-data-table > .v-data-table__wrapper > table > thead > tr:last-child > th {
    color: #d3d3d3 !important;
  }
</style>
