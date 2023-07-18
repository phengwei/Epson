<template>
  <v-data-table :headers="headers"
                :items="itemsPendingFulfilment"
                :loading="loading"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>New Request</v-toolbar-title>
        <v-divider class="mx-4" inset vertical></v-divider>
        <v-spacer></v-spacer>

        <ProductFulfillmentDialog :editedItem="editedItem"
                                  :dialogProductFulfillment="dialogProductFulfillment"
                                  @close="close"></ProductFulfillmentDialog>
      </v-toolbar>
    </template>

    <template v-slot:item.actions="{ item }">
      <v-icon small
              class="mr-2"
              @click="editItem(item)">
        mdi-pencil
      </v-icon>
      <v-btn @click="viewRequest(item)">View</v-btn>
    </template>
  </v-data-table>
</template>

<script>
  import ProductFulfillmentDialog from '~/components/ProductFulfillmentDialog.vue';
  export default {
    name: 'ItemsPendingFulfilmentTable',
    components: {
      ProductFulfillmentDialog
    },
    data() {
      return {
        dialogProductFulfillment: false,
        headers: [
          {
            text: 'ID',
            align: ' d-none',
            value: 'id',
          },
          { text: 'Requested By', value: 'createdBy' },
          { text: 'Customer', value: 'customerName' },
          { text: 'Product', value: 'productName' },
          { text: 'Budget', value: 'budget' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Fulfill Request', value: 'actions', sortable: false },
        ],
        itemsPendingFulfilment: [],
        productsToShow: [],
        competitorsToShow: [],
        loading: false,
        editedItem: {},
      }
    },
    watch: {
      options: {
        handler() {
          this.getFulfillerItem()
        },
        deep: true,
      },
      dialogProductFulfillment(val) {
        val || this.close()
      }
    },
    created() {
      this.getFulfillerItem();
    },

    methods: {
      viewRequest(request) {
        this.$router.push({
          path: '/createquotation',
          query: { view: true, isFulfill: true, request: JSON.stringify(request) }
        });
      },
      today() {
        const date = new Date();
        const year = date.getFullYear();
        const month = ('0' + (date.getMonth() + 1)).slice(-2);
        const day = ('0' + date.getDate()).slice(-2);
        const hours = ('0' + date.getHours()).slice(-2);
        const minutes = ('0' + date.getMinutes()).slice(-2);

        return `${year}-${month}-${day}T${hours}:${minutes}`;
      },
      getFulfillerItem() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfilleritem`).then(result => {
          this.itemsPendingFulfilment = [];
          result.data.data.forEach(item => {
            item.requestProductsModel.forEach(product => {
              const newItem = {
                ...item,
                ...product,
                productName: product.productName,
                budget: product.budget,
                quantity: product.quantity,
                competitors: [],
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
              const p = {
                quantity: product.quantity,
                budget: product.budget,
                productName: product.productName,
                tenderDate: product.tenderDate,
                remarks: product.remarks
              };
              this.productsToShow.push(p);
            });
          });
          this.loading = false
        })
      },
      editItem(item) {
        this.editedItem = { ...item, deliveryDate: this.today() };
        this.dialogProductFulfillment = true;
      },
      close() {
        this.dialogProductFulfillment = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        })
      },
    },
  }
</script>
