import moment from 'moment';
import Swal from 'sweetalert2';
import ProductDialog from '~/components/ProductDialog.vue';
import CompetitorInformationDialog from '~/components/CompetitorInformationDialog.vue';
import CoverplusDialog from '~/components/CoverplusDialog.vue';
import ProductFulfillmentDialog from '~/components/ProductFulfillmentDialog.vue';
import { ApprovalStateEnum } from '~/script/approvalStateEnum.js';

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
      product: { category: null, productId: null, quantity: null, distyPrice: null, dealerPrice: null, endUserPrice: null, remarks: null },
      products: [],
      coverplus: { category: null, productId: null, quantity: null, distyPrice: null, dealerPrice: null, endUserPrice: null },
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
      nonCoverplusRequestItem: {},
      coverplusRequestItem: {},
      itemsPendingFulfillment: [],
      currentRequest: {},
      ApprovalStateEnum,
      editedItem: {},
    };
  },
  async created() {
    this.submissionDetail.createdOnUTC = this.getToday();
    await this.fetchCategories();
    if (this.$route.query.view || this.$route.query.editable) {
      console.log("populate");
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
    currentRequestApprovalState() {
      return this.currentRequest ? this.currentRequest.approvalState : null;
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
    isMode(mode) {
      return this.$route.query[mode] === 'true';
    },
    confirmApproveRequest() {
      Swal.fire({
        title: 'Confirmation',
        text: 'Are you sure you want to approve the quotation?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes',
        cancelButtonText: 'No'
      }).then((result) => {
        if (result.isConfirmed) {
          this.$axios.post(`${this.$config.restUrl}/api/request/approvefirstlevelrequest?requestId=${this.currentRequest.id}`)
            .then(response => {
              this.closeDialogProductFulfillment();
              Swal.fire('Amended!', 'Request is successfully approved.', 'success')
                .then(() => {
                  this.$router.push('/request');
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to approve quotation', 'error');
            });
        }
      });
    },

    closeDialogProductFulfillment() {
      this.closeDialogProductFulfillment = false;
    },
    fulfillNonCoverplusItem() {
      this.editedItem = { ...this.nonCoverplusRequestItem[0], deliveryDate: this.getToday() };
      this.dialogProductFulfillment = true;
    },
    fulfillCoverplusItem() {
      this.editedItem = { ...this.coverplusRequestItem[0], deliveryDate: this.getToday() };
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
      this.product.category = null;
      this.product.productId = null;
      this.product.quantity = null;
      this.product.distyPrice = null;
      this.product.dealerPrice = null;
      this.product.endUserPrice = null;
      this.product.tenderDate = null;
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
      this.product.distyPrice = null;
      this.product.dealerPrice = null;
      this.product.endUserPrice = null;
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
      this.currentRequest = requestData;
      for (const productModel of requestData.requestProductsModel) {
        const categoryFound = this.categories.find((categoryFound) => categoryFound.id === productModel.productCategory.categoryId);
        if (categoryFound) {
          this.selectedCategories.push(categoryFound);
          await this.fetchProductsForCategory(categoryFound);
          const newItem = {
            ...requestData,
            ...productModel,
            productName: productModel.productName,
            distyPrice: productModel.distyPrice,
            dealerPrice: productModel.dealerPrice,
            endUserPrice: productModel.endUserPrice,
            quantity: productModel.quantity,
            isCoverplus: productModel.isCoverplus,
          };

          this.itemsPendingFulfillment.push(newItem);
          this.nonCoverplusRequestItem = this.itemsPendingFulfillment.filter(item => !item.isCoverplus);
          this.coverplusRequestItem = this.itemsPendingFulfillment.filter(item => item.isCoverplus);

          const p = {
            category: categoryFound,
            productid: productModel.productId,
            quantity: productModel.quantity,
            distyPrice: productModel.distyPrice,
            dealerPrice: productModel.dealerPrice,
            endUserPrice: productModel.endUserPrice,
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
          distyPrice: this.productsToShow[product].distyPrice,
          dealerPrice: this.productsToShow[product].dealerPrice,
          endUserPrice: this.productsToShow[product].endUserPrice,
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
          distyPrice: this.coverplusesToShow[coverplus].distyPrice,
          dealerPrice: this.coverplusesToShow[coverplus].dealerPrice,
          endUserPrice: this.coverplusesToShow[coverplus].endUserPrice,
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
