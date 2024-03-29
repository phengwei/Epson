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

    <template v-slot:item="{ item }">
      <tr v-if="item.authorizedToFulfill && item.status === RequestProductStatusEnum.Pending">
        <td>{{ item.requestId }}</td>
        <td>{{ item.createdOnUTC }}</td>
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
  import ProductFulfillmentDialog from '~/components/ProductFulfillmentDialog.vue';
  import { RequestProductStatusEnum } from '~/script/requestProductStatusEnum.js';
  export default {
    name: 'ItemsPendingFulfilmentTable',
    components: {
      ProductFulfillmentDialog
    },
    data() {
      return {
        dialogProductFulfillment: false,
        headers: [
          { text: 'Request #', value: 'id' },
          { text: 'Requested On', value: 'createdOnUTC' },
          { text: 'Requested By', value: 'createdBy' },
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
        RequestProductStatusEnum
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
        request.requestProductsModel = request.requestProductsModel.filter(product => product.productId === request.productId);

        let queryParameters = { view: true, request: JSON.stringify(request) };

        if (request.isCoverplus === true) {
          queryParameters = { ...queryParameters, isFulfillCoverplus: true };
        } else if (request.isCoverplus === false) {
          queryParameters = { ...queryParameters, isFulfill: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
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
            console.log("item", item);
            item.requestProductsModel.forEach(product => {
              const newItem = {
                ...item,
                ...product,
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
              const p = {
                quantity: product.quantity,
                distyPrice: product.distyPrice,
                dealerPrice: product.dealerPrice,
                endUserPrice: product.endUserPrice,
                productName: product.productName,
                remarks: product.remarks
              };
              this.productsToShow.push(p);
            });
          });
          this.loading = false;
        })
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
