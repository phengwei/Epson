<template>
  <div class="create-quotation-container">
    <h1>Pricing Request</h1>
    <v-card class="mx-auto" width="800">
      <v-card-text>
        <ProductDialog :dialogProduct.sync="dialogProduct"
                       :product="product"
                       :isViewMode="isViewMode"
                       @add-product="addProductRow" />
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions mb-4">
              <v-btn v-if="!isViewMode" color="primary" @click="dialogProduct = true">
                Add New
              </v-btn>
            </div>
            <table class="mb-5 mt-2">
              <thead>
                <tr class="header-row">
                  <th colspan="3"><h2>PROPOSED MODEL</h2></th>
                  <th colspan="5"><h2>PRICE EXPECTATION (RM)</h2></th>
                </tr>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Budget</th>
                  <th v-if="isViewMode">Remarks</th>
                  <th>Tender Date</th>
                  <th v-if="isViewMode">Delivery Date</th>
                  <th v-if="isViewMode">Status</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(product, index) in productsToShow" :key="index">
                  <td>{{ product.category ? product.category.name : 'N/A' }}</td>
                  <td>{{ product.productId ? findProductName(product.productId) : product.productName }}</td>
                  <td>{{ product.quantity || 'N/A' }}</td>
                  <td>{{ product.budget || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ product.remarks || 'N/A' }}</td>
                  <td>{{ product.tenderDate || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ product.deliveryDate || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ product.statusStr || 'N/A' }}</td>
                  <td v-if="!isViewMode">
                    <v-btn small color="error" @click="removeProduct(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <CoverplusDialog :dialogCoverplus.sync="dialogCoverplus"
                       :coverplus="coverplus"
                       :isViewMode="isViewMode"
                       @add-coverplus="addCoverplusRow" />
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions mb-4">
              <v-btn v-if="!isViewMode" color="primary" @click="dialogCoverplus = true">
                Add New
              </v-btn>
            </div>
            <table class="mb-5 mt-2">
              <thead>
                <tr class="header-row">
                  <th colspan="3"><h2>PROPOSED COVERPLUS</h2></th>
                  <th colspan="2"><h2>PRICE EXPECTATION (RM)</h2></th>
                </tr>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Budget</th>
                  <th v-if="isViewMode">Status</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(coverplus, index) in coverplusesToShow" :key="index">
                  <td>{{ coverplus.category ? coverplus.category.name : 'N/A' }}</td>
                  <td>{{ coverplus.productId ? findProductName(coverplus.productId) : coverplus.productName }}</td>
                  <td>{{ coverplus.quantity || 'N/A' }}</td>
                  <td>{{ coverplus.budget || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ coverplus.statusStr || 'N/A' }}</td>
                  <td v-if="!isViewMode">
                    <v-btn small color="error" @click="removeCoverplus(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <CompetitorInformationDialog :dialogCompetitor.sync="dialogCompetitor"
                                     :competitor="competitor"
                                     :isViewMode="isViewMode"
                                     @add-competitor="addCompetitorRow" />
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions mb-4">
              <v-btn v-if="!isViewMode" color="primary" @click="dialogCompetitor = true">
                Add New
              </v-btn>
            </div>
            <table class="mb-5 mt-2">
              <thead>
                <tr class="header-row">
                  <th colspan="4"><h2>COMPETITOR'S INFORMATION</h2></th>
                </tr>
                <tr>
                  <th>Model</th>
                  <th>Brand</th>
                  <th>Price</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(competitor, index) in competitorsToShow" :key="index">
                  <td>{{ competitor.model }}</td>
                  <td>{{ competitor.brand }}</td>
                  <td>{{ competitor.price }}</td>
                  <td v-if="!isViewMode">
                    <v-btn small color="error" @click="removeCompetitorInformation(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <div class="form-group">
          <label>Customer Name</label>
          <input type="text" v-model="customerName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
        <div class="form-group">
          <label>Priority</label>
          <select v-model="priority.value" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
            <option v-for="option in priority.options" :value="option.value" :key="option.value">
              {{ option.label }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Requirements</label>
          <textarea v-model="dealJustification" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></textarea>
        </div>
        <div class="form-group">
          <label>Deadline</label>
          <input type="datetime-local" v-model="deadline" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
        <div class="form-group" v-if="comments != ''">
          <label>Comments</label>
          <textarea v-model="comments" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></textarea>
        </div>
        <button type="submit" @click="submitQuotation" v-if="!isViewMode">Submit</button>
        <button type="submit" @click="saveDraft" v-if="!isViewMode">Save Draft</button>
        <button type="submit" @click="redirectToRequest">Return to Request</button>
      </v-card-text>
    </v-card>

  </div>
</template>

<script>
  import moment from 'moment';
  import ProductDialog from '~/components/ProductDialog.vue';
  import CompetitorInformationDialog from '~/components/CompetitorInformationDialog.vue';
  import CoverplusDialog from '~/components/CoverplusDialog.vue';

  export default {
    name: "request-quotation",
    components: {
      ProductDialog,
      CompetitorInformationDialog,
      CoverplusDialog
    },
    data() {
      return {
        categories: [],
        selectedCategories: [],
        isChecked: [],
        selectedProducts: {},
        product: { category: null, productId: null, quantity: null, budget: null, remarks: null, tenderDate: null, deliveryDate: null },
        products: [],
        coverplus: { category: null, productId: null, quantity: null, budget: null },
        coverpluses: [],
        competitor: { model: null, brand: null, price: null },
        competitors: [],
        productsToShow: [],
        competitorsToShow: [],
        coverplusesToShow: [],
        options: {},
        priority: {
          value: 1,
          options: [
            { value: 1, label: 'High' },
            { value: 2, label: 'Medium' },
            { value: 3, label: 'Low' }
          ]
        },
        quantity: {},
        budget: {},
        fulfilledPrice: {},
        fulfillerName: {},
        approvalStateStr: '',
        customerName: '',
        dealJustification: '',
        deadline: '',
        dialogProduct: false,
        dialogCompetitor: false,
        dialogCoverplus: false,
        comments: ''
      };
    },
    async created() {
      await this.fetchCategories();
      if (this.$route.query.view) {
        const request = JSON.parse(this.$route.query.request);
        this.populateForm(request);
      }
      else {
        await this.loadDraft();
      }
    },
    computed: {
      isViewMode() {
        return this.$route.query.view === 'true';
      },
      today() {
        const date = new Date();
        const year = date.getFullYear();
        let month = date.getMonth() + 1;
        let day = date.getDate();
        let hours = date.getHours();
        let minutes = date.getMinutes();

        month = (month < 10) ? `0${month}` : month;
        day = (day < 10) ? `0${day}` : day;
        hours = (hours < 10) ? `0${hours}` : hours;
        minutes = (minutes < 10) ? `0${minutes}` : minutes;

        return `${year}-${month}-${day}T${hours}:${minutes}`;
      }
    },
    methods: {
      addCompetitorRow(competitor) {
        const newCompetitor = { ...competitor };
        this.competitors.push(newCompetitor);
        this.showAddedCompetitors(newCompetitor);
        this.competitor.brand = null;
        this.competitor.model = null;
        this.competitor.price = null;
        this.dialogCompetitor = false;
      },
      removeCompetitorInformation(index) {
        this.competitorsToShow.splice(index, 1);
      },
      showAddedCompetitors(newCompetitor) {
        this.competitorsToShow.push(newCompetitor);
      },
      addCoverplusRow(coverplus) {
        const newCoverplus = { ...coverplus };
        this.coverpluses.push(newCoverplus);
        this.showAddedCoverpluses(newCoverplus);
        this.coverplus.brand = null;
        this.coverplus.model = null;
        this.coverplus.price = null;
        this.dialogCoverplus = false;
      },
      removeCoverplusInformation(index) {
        this.coverplusesToShow.splice(index, 1);
      },
      showAddedCoverpluses(newCoverplus) {
        this.coverplusesToShow.push(newCoverplus);
      },
      addProductRow(product) {
        const newProduct = { ...product };
        this.products.push(newProduct);
        this.showAddedProducts(newProduct);
        this.product.category = null;
        this.product.productId = null;
        this.product.quantity = null;
        this.product.budget = null;
        this.product.tenderDate = null;
        this.dialogProduct = false;
      },
      removeProduct(index) {
        this.productsToShow.splice(index, 1);
      },
      showAddedProducts(newProduct) {
        this.productsToShow.push(newProduct);
      },
      findProductName(productId) {
        for (const category of this.categories) {
          for (const product of category.products) {
            if (product.id === productId) {
              return product.name;
            }
          }
        }
        return 'N/A';
      },
      async populateForm(requestData) {
        for (const productModel of requestData.requestProductsModel) {
          const categoryFound = this.categories.find((categoryFound) => categoryFound.id === productModel.productCategory.categoryId);
          if (categoryFound) {
            console.log("prod", productModel);
            this.selectedCategories.push(categoryFound);
            await this.fetchProductsForCategory(categoryFound);
            const p = {
              category: categoryFound,
              productid: productModel.productId,
              quantity: productModel.quantity,
              budget: productModel.budget,
              productName: productModel.productName,
              tenderDate: productModel.tenderDate === "0001-01-01T00:00:00" ? "N/A" : moment(productModel.tenderDate).format('MMMM Do YYYY'),
              deliveryDate: productModel.deliveryDate === "0001-01-01T00:00:00" ? "N/A" : moment(productModel.deliveryDate).format('MMMM Do YYYY'),
              remarks: productModel.remarks,
              statusStr: productModel.statusStr
            };
            if (productModel.isCoverplus === true) {
              this.coverplusesToShow.push(p);
            } else {
              this.productsToShow.push(p);
            }
          }
        }
        for (const competitorModel of requestData.competitorInformationModel) {
          const c = {
            model: competitorModel.model,
            brand: competitorModel.brand,
            price: competitorModel.price
          };
          this.competitorsToShow.push(c);
        }
        this.priority.value = requestData.priority;
        this.customerName = requestData.customerName;
        this.dealJustification = requestData.dealJustification;
        this.deadline = requestData.deadline;
        this.approvalStateStr = requestData.approvalStateStr;
        this.deliveryDate = requestData.deliveryDate;
        this.tenderDate = requestData.tenderDate;
        this.comments = requestData.comments;
      },
      async fetchCategories() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`);
          this.categories = response.data.data;
          for (const category of this.categories) {
            await this.fetchProductsForCategory(category);
          }
        } catch (error) {
          console.error(error);
        }
      },
      async fetchProductsForCategory(category) {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
          this.$set(this.options, category.id, response.data.data);
        } catch (error) {
          console.error(error);
        }
      },
      loadDraft() {
        try {
          this.selectedCategories = [];
          this.productsToShow = [];

          this.productsToShow = JSON.parse(localStorage.getItem("savedItem-productsToShowList")) || [];
          this.competitorsToShow = JSON.parse(localStorage.getItem("savedItem-competitorsToShowList")) || [];
          this.customerName = localStorage.getItem("savedItem-customerName", this.customerName) || "";
          this.priority.value = localStorage.getItem("savedItem-priority", this.priority.value);
          this.dealJustification = localStorage.getItem("savedItem-dealJustification", this.dealJustification) || "";
          this.deadline = localStorage.getItem("savedItem-deadline", this.deadline);

        } catch (error) {
          console.error(error);
        }
      },
      saveDraft() {

        localStorage.clear();

        localStorage.setItem("savedItem-productsToShowList", JSON.stringify(this.productsToShow));
        localStorage.setItem("savedItem-competitorsToShowList", JSON.stringify(this.competitorsToShow));
        localStorage.setItem("savedItem-customerName", this.customerName);
        localStorage.setItem("savedItem-priority", this.priority.value);
        localStorage.setItem("savedItem-dealJustification", this.dealJustification);
        localStorage.setItem("savedItem-deadline", this.deadline);
        this.$swal('Request draft saved');

      },
      checkboxChanged(selectedCategory) {
        if (selectedCategory) {
          const categoryId = selectedCategory.id;

          if (this.selectedCategories.includes(selectedCategory)) {
            if (this.selectedProducts[categoryId] === undefined) {
              this.selectedProducts[categoryId] = '';
              this.quantity[categoryId] = null;
              this.budget[categoryId] = null;
            }
            this.submitForm(selectedCategory);
          } else {
            this.$delete(this.selectedProducts, categoryId);
            this.$delete(this.quantity, categoryId);
            this.$delete(this.budget, categoryId);
            this.$delete(this.options, categoryId);
          }
        }
      },
      async submitForm(selectedCategory) {
        if (selectedCategory.id != null) {
          const categoryId = selectedCategory.id;
          const formValues = {
            categoryId,
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: formValues });
            this.$set(this.options, categoryId, response.data.data);
          } catch (error) {
            console.error(error);
          }
        }

      },
      redirectToRequest() {
        this.$router.push('/request');
      },
      async submitQuotation() {
        if (this.deadline < this.today) {
          this.$swal('Error', 'Deadline should be later than today', 'error');
          return;
        }
        const quotationData = {
          ApprovalState: 20,
          Priority: this.priority,
          requestProducts: [],
          competitorInformations: [],
        };
        for (const product in this.productsToShow) {
          const productToInsert = {
            productId: this.productsToShow[product].productId,
            quantity: this.productsToShow[product].quantity,
            budget: this.productsToShow[product].budget,
            tenderDate: this.productsToShow[product].tenderDate,
            isCoverplus: false
          };
          quotationData.requestProducts.push(productToInsert);
        }
        for (const competitor in this.competitorsToShow) {
          const competitorToInsert = {
            model: this.competitorsToShow[competitor].model,
            brand: this.competitorsToShow[competitor].brand,
            price: this.competitorsToShow[competitor].price
          };
          quotationData.competitorInformations.push(competitorToInsert);
        }
        for (const coverplus in this.coverplusesToShow) {
          const coverplusToInsert = {
            productId: this.coverplusesToShow[coverplus].productId,
            quantity: this.coverplusesToShow[coverplus].quantity,
            budget: this.coverplusesToShow[coverplus].budget,
            isCoverplus: true
          };
          quotationData.requestProducts.push(coverplusToInsert);
        }
        try {
          const vm = this;
          await this.$axios.post(`${this.$config.restUrl}/api/request/createrequest`, {
            data: {
              segment: "string",
              approvalState: 10,
              priority: this.priority.value,
              RequestProducts: quotationData.requestProducts,
              CompetitorInformations: quotationData.competitorInformations,
              customerName: this.customerName,
              dealJustification: this.dealJustification,
              deadline: this.deadline,
              comments: '',
            }
          }).then(response => {
            this.$swal('Request created');
            this.$router.push('/request');
            localStorage.clear();
          }).catch(err => {
            console.log(err);
            vm.$swal('Failed to submit request', err.response.data.message, 'error');
          })
        } catch (error) {
          console.log(error);
        }
      }
    }
  }
</script>

<style scoped>
  .products-title {
    font-size: 2em;
    text-align: center;
  }

  .header-row {
    background-color: #C0C0C0;
  }
    .header-row th {
      text-align: center;
      vertical-align: middle;
    }

  table {
    width: 100%;
    margin-top: 2rem;
    border-collapse: collapse;
  }

  th, td {
    border: 1px solid #ddd;
    padding: 8px;
    text-align: left;
  }

  h1 {
    margin-top: 0;
    font-size: 2rem;
    text-align: center;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 0.5rem;
    width: 100%;
  }

  .form-container {
    max-width: 400px;
    padding: 2rem;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .create-quotation-container {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
  }

  label {
    font-weight: bold;
    margin-bottom: 0.5rem;
    color: black;
  }

  .blue-checkbox {
    margin-bottom: 1rem;
  }

    .blue-checkbox input[type="checkbox"]:checked {
      background-color: #4285f4;
      border-color: #4285f4;
    }

  input[type="checkbox"] {
    margin-right: 0.5rem;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;
    border-radius: 3px;
    border: 2px solid #ccc;
    width: 1.2em;
    height: 1.2em;
    margin-left: 5%
  }

  .form-group {
    margin-bottom: 1rem;
    display: flex;
    justify-content: center;
    flex-direction: column;
  }

  button {
    padding: 0.5rem 1rem;
    background-color: #003399;
    color: #fff;
    border: none;
    cursor: pointer;
  }

  .readonly-field {
    background-color: #ddd;
  }

  @media (max-width: 768px) {
    form {
      max-width: 300px;
      padding: 1rem;
    }

    h1 {
      font-size: 1.5rem;
    }
  }
</style>
