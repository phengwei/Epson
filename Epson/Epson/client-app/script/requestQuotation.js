import moment from 'moment';
import ProductDialog from '~/components/ProductDialog.vue';
import CompetitorInformationDialog from '~/components/CompetitorInformationDialog.vue';
import CoverplusDialog from '~/components/CoverplusDialog.vue';
import ProductFulfillmentDialog from '~/components/ProductFulfillmentDialog.vue';

export default {
  name: "request-quotation",
  components: {
    ProductDialog,
    CompetitorInformationDialog,
    CoverplusDialog,
    ProductFulfillmentDialog
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
      submissionDetail: { preparedBy: null, createdOnUTC: null, distributorName: null, resellerName: null, contactPersonName: null, telephoneNo: null, faxNo: null, email: null },
      projectInformation: {
        projectName: null, projectId: null, industry: null, type: null, closingDate: null, deliveryDate: null, companyAddress: null, contactPersonName: null,
        email: null, requirements: null, budget: null, staggeredDelivery: null, otherInformation: null,
        projectInformationReason: {
          id: null, projectInformationId: null, selectedReasons: null, additionalInfo: null
        },
        projectInformationReasons: []
      },
      projectInformationReasonsToInsert: [],
      reasons: [
        { text: "New Purchase", info: "" },
        { text: "Additional Purchase", info: "Quotation No." },
        { text: "Renewal of Quotation", info: "Quotation No." },
        { text: "Revision (Price/Model/Qty/Other)", info: "Quotation No." },
        { text: "Replacement of old machine", info: "" }
      ],
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
      dialogProductFulfillment: false,
      comments: '',
      requestItem: {},
      editedItem: {},
    };
  },
  async created() {
    this.submissionDetail.createdOnUTC = this.getToday();
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
    isFulfillMode() {
      return this.$route.query.isFulfill === 'true';
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
    close() {
      this.dialogProductFulfillment = false
      this.$nextTick(() => {
        this.editedItem = Object.assign({}, this.defaultItem)
        this.editedIndex = -1
      })
    },
    fulfillItem() {
      this.editedItem = { ...this.requestItem, deliveryDate: this.getToday() };
      this.competitorsToShow = [...this.requestItem.competitors];
      this.dialogProductFulfillment = true;
    },
    getToday() {
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
    },
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
      this.requestItem = requestData;
      for (const productModel of requestData.requestProductsModel) {
        const categoryFound = this.categories.find((categoryFound) => categoryFound.id === productModel.productCategory.categoryId);
        if (categoryFound) {
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
      this.submissionDetail = requestData.requestSubmissionDetailModel;
      this.projectInformation = requestData.projectInformationModel;
      this.projectInformation.projectInformationReasons = requestData.projectInformationModel.projectInformationReasons.map(reasonObject => reasonObject.selectedReason);
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
    handleCheckboxChange(event, reason) {
      if (event.target.checked) {
        this.projectInformationReasonsToInsert.push({
          id: 0,
          projectInformationId: 0,
          selectedReason: reason,
          additionalInfo: null
        });
      } else {
        const index = this.projectInformationReasonsToInsert.findIndex(r => r.selectedReason === reason);
        if (index !== -1) {
          this.projectInformationReasonsToInsert.splice(index, 1);
        }
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
      quotationData.submissionDetail = {
        createdOnUTC: this.submissionDetail.createdOnUTC,
        distributorName: this.submissionDetail.distributorName,
        resellerName: this.submissionDetail.resellerName,
        contactPersonName: this.submissionDetail.contactPersonName,
        telephoneNo: this.submissionDetail.telephoneNo,
        faxNo: this.submissionDetail.faxNo,
        email: this.submissionDetail.email
      }
      quotationData.projectInformation = {
        projectName: this.projectInformation.projectName,
        projectId: this.projectInformation.projectId,
        industry: this.projectInformation.industry,
        type: this.projectInformation.type,
        closingDate: this.projectInformation.closingDate,
        deliveryDate: this.projectInformation.deliveryDate,
        companyAddress: this.projectInformation.companyAddress,
        contactPersonName: this.projectInformation.contactPersonName,
        telephoneNo: this.projectInformation.telephoneNo,
        email: this.projectInformation.email,
        requirements: this.projectInformation.requirements,
        customerApplications: this.projectInformation.customerApplications,
        budget: this.projectInformation.budget,
        staggeredDelivery: this.projectInformation.staggeredDelivery,
        otherInformation: this.projectInformation.otherInformation,
        projectInformationReasons: this.projectInformationReasonsToInsert
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
            requestSubmissionDetail: quotationData.submissionDetail,
            ProjectInformationModel: quotationData.projectInformation,
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
