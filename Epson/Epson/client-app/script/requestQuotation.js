import { mapGetters } from 'vuex';
import { Base64 } from 'js-base64';
import moment from 'moment';
import Swal from 'sweetalert2';
import JsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { ApprovalStateEnum } from '~/script/approvalStateEnum.js';

const statusMapping = {
  0: 'Pending',
  10: 'Cancelled',
  20: 'Rejected',
  30: 'Approved',
  40: 'Pending Division Head Approval'
};

export default {
  name: "request-quotation",
  components: {
    ProductDialog: () => import('~/components/ProductDialog.vue'),
    CompetitorInformationDialog: () => import('~/components/CompetitorInformationDialog.vue'),
    CoverplusDialog: () => import('~/components/CoverplusDialog.vue'),
    ProductFulfillmentDialog: () => import('~/components/ProductFulfillmentDialog.vue'),
  },
  watch: {
    reasons: {
      handler(newVal) {
        newVal.forEach(reason => {
          if (reason.isChecked && (reason.text === 'Additional Purchase' || reason.text === 'Renewal of Quotation' || reason.text === 'Revision (Price/Model/Qty/Other)')) {
            const reasonToInsert = this.projectInformationReasonsToInsert.find(r => r.selectedReason === reason.text);
            if (reasonToInsert) {
              reasonToInsert.additionalInfo = reason.additionalText;
            }
          }
        });
      },
      deep: true
    }
  },
  data() {
    return {
      slaTypes: ['Local', 'Regional', 'SEC'],
      categories: [],
      selectedCategories: [],
      isChecked: [],
      selectedProducts: [],
      selectedCoverpluses: [],
      months: ['None', 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
      product: { category: null, productId: null, quantity: null, distyPrice: null, dealerPrice: null, endUserPrice: null, remarks: null },
      products: [],
      coverplus: { category: null, productId: null, quantity: null, distyPrice: null, dealerPrice: null, endUserPrice: null, remarks: null, warrantyRequest: null, warrantyRequestPeriod: null, status: 0 },
      coverpluses: [],
      competitor: { model: null, brand: null, distyPrice: null, dealerPrice: null, endUserPrice: null },
      competitors: [],
      productsToShow: [],
      competitorsToShow: [],
      coverplusesToShow: [],
      submissionDetail: { createdByStr: null, createdOnUTC: null, distributorName: null, resellerName: null, contactPersonName: null, telephoneNo: null, faxNo: null, email: null },
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
        { text: "New Purchase", info: "", isChecked: false, additionalText: "" },
        { text: "Additional Purchase", info: "Quotation No.", isChecked: false, additionalText: "" },
        { text: "Renewal of Quotation", info: "Quotation No.", isChecked: false, additionalText: "" },
        { text: "Revision (Price/Model/Qty/Other)", info: "Quotation No.", isChecked: false, additionalText: "" },
        { text: "Replacement of old machine", info: "", isChecked: false, additionalText: "" },
        { text: "Demo Unit Price Requisition", info: "", isChecked: false, additionalText: "" }
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
      distributors: ['Servex', 'Ingram', 'VSTECs', 'Etech IT', 'GOS', 'EDAP'],
      quantity: {},
      budget: {},
      fulfilledPrice: {},
      fulfillerName: {},
      approvalStateStr: '',
      customerName: '',
      dealJustification: '',
      deadline: '',
      dialogProduct: false,
      selectedProduct: null,
      selectedCoverplus: null,
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
      submitting: false,
      loading: false,
      decodedQueryParams: {},
      currentDate: new Date().toISOString().slice(0, 16)
    };
  },

  async created() {
    this.loading = true;
    this.submissionDetail.createdOnUTC = this.getToday();
    this.submissionDetail.createdByStr = this.loggedInUser.userName;
    await this.fetchCategories();

    if (this.$route.query.params) {
      this.decodedQueryParams = JSON.parse(Base64.decode(this.$route.query.params));
      const queryParams = JSON.parse(Base64.decode(this.$route.query.params));
      console.log("awd", queryParams);
      if (this.decodedQueryParams.view || this.decodedQueryParams.editable) {
        const requestId = queryParams.requestId;
        await this.fetchRequestById(requestId);
        this.populateForm(this.unpopulatedRequests);
      }
    }
    this.loading = false;
  },
  computed: {
    ...mapGetters(['isAuthenticated', 'loggedInUser']),
    formattedRequestDate() {
      return moment(this.submissionDetail.createdOnUTC).format('DD/MM/YYYY hh:mm A');
    },
    formattedApprovedTime() {
      return moment(this.approvedTime).format('DD/MM/YYYY hh:mm A');
    },
    formattedClosingDate() {
      return moment(this.projectInformation.closingDate).format('DD/MM/YYYY hh:mm A');
    },
    formattedDeliveryDate() {
      return moment(this.projectInformation.deliveryDate).format('DD/MM/YYYY hh:mm A');
    },
    isCommentEditable() {
      return this.isViewMode() && !this.isMode('dealable');
    },
    isViewMode() {
      return this.decodedQueryParams.view === true;
    },
    isAmendMode() {
      return this.decodedQueryParams.editable === true;
    },
    isFulfillMode() {
      return this.decodedQueryParams.isFulfill === true || this.decodedQueryParams.isFulfillCoverplus === true;
    },
    isFinalApproveMode() {
      return this.decodedQueryParams.isFinalApprove === true
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
    fulfillSelectedProducts() {
      // Check if all selected products have an SLA type
      const missingSla = this.selectedProducts.some(productId => {
        const product = this.productsToShow.find(p => p.id === productId);
        return !product || !product.sla;
      });

      if (missingSla) {
        this.$swal('Error', 'Please select an SLA Type for all selected products before fulfilling.', 'error');
        return;
      }

      // Existing fulfillment logic
      this.$swal({
        title: 'Fulfill Requests?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, fulfill it!'
      }).then((result) => {
        if (result.isConfirmed) {
          const payload = this.selectedProducts.map(productId => {
            const product = this.productsToShow.find(p => p.id === productId);
            return {
              id: productId,
              sla: product.sla
            };
          });

          this.fulfillRequests(payload);
        }
      });
    },
    fulfillSelectedCoverpluses() {
      // Check if all selected coverpluses have an SLA type
      const missingSla = this.selectedCoverpluses.some(coverplusId => {
        const coverplus = this.coverplusesToShow.find(c => c.id === coverplusId);
        return !coverplus || !coverplus.sla;
      });

      if (missingSla) {
        this.$swal('Error', 'Please select an SLA Type for all selected coverpluses before fulfilling.', 'error');
        return;
      }

      // Existing fulfillment logic
      this.$swal({
        title: 'Fulfill Requests?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, fulfill it!'
      }).then((result) => {
        if (result.isConfirmed) {
          const payload = this.selectedCoverpluses.map(coverplusId => {
            const coverplus = this.coverplusesToShow.find(c => c.id === coverplusId);
            return {
              id: coverplusId,
              sla: coverplus.sla
            };
          });

          this.fulfillRequests(payload);
        }
      });
    },
    async fulfillRequests(payload) {
      try {
        await this.$axios.post(`${this.$config.restUrl}/api/request/fulfillrequests`, payload);
        this.$swal('Success', 'Request fulfilled successfully.', 'success').then(() => {
          location.reload();
        });
      } catch (error) {
        console.error('Error fulfilling requests:', error);
        this.$swal('Failed to fulfill request', error.response.data.message, 'error');
      }
    },
    isMode(mode) {
      return this.decodedQueryParams[mode] === true;
    },
    openFulfillProductDialog(product) {
      console.log("product", product);
      this.editedItem = { ...product };
      this.editedItem.createdByStr = this.submissionDetail.createdByStr;
      this.dialogProductFulfillment = true;
    },
    openAddProductDialog() {
      this.selectedProduct = {};
      this.dialogProduct = true;
    },
    openEditProductDialog(product) {
      console.log("product", product);
      this.selectedProduct = { ...product };
      this.$nextTick(() => {
        this.dialogProduct = true;
        this.$refs.productDialog.setEditMode(true, product);
      });
    },
    editProductRow(editedProduct) {
      const index = this.productsToShow.findIndex(product => product.productId === editedProduct.productId);
      if (index !== -1) {
        const updatedProduct = { ...editedProduct };
        updatedProduct.distyPrice = updatedProduct.distyPrice || 0;
        updatedProduct.dealerPrice = updatedProduct.dealerPrice || 0;
        updatedProduct.endUserPrice = updatedProduct.endUserPrice || 0;

        this.$set(this.productsToShow, index, updatedProduct);
      } else {
        console.error("Product not found for editing.");
      }

      this.dialogProduct = false;
    },
    addProductRow(product) {
      const newProduct = { ...product, slaType: 'Local' };
      this.products.push(newProduct);
      this.showAddedProducts(newProduct);
      this.product = {
        category: null,
        productId: null,
        quantity: null,
        distyPrice: null,
        dealerPrice: null,
        endUserPrice: null,
        remarks: null,
        slaType: 'Local'
      };
      this.dialogProduct = false;
    },
    confirmAmmendQuotation() {
      Swal.fire({
        title: 'Confirmation',
        text: 'Are you sure you want to amend the quotation?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes',
        cancelButtonText: 'No'
      }).then((result) => {
        if (result.isConfirmed) {
          this.$axios.post(`${this.$config.restUrl}/api/request/setrequesttoamendquotation?requestId=${this.currentRequest.id}`)
            .then(response => {
              Swal.fire('Amended!', 'Request is in amend stage.', 'success')
                .then(() => {
                  window.location.href = '/request';
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to set request to amend stage', 'error');
            });
        }
      });
    },
    approveQuotation() {
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
              Swal.fire('Approved!', 'Quotation is successfully approved.', 'success')
                .then(() => {
                  window.location.href = '/request';
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to approve quotation', 'error');
            });
        }
      });
    },
    rejectQuotation() {
      Swal.fire({
        title: 'Confirmation',
        text: 'Are you sure you want to reject the quotation?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes',
        cancelButtonText: 'No'
      }).then((result) => {
        if (result.isConfirmed) {
          this.$axios.post(`${this.$config.restUrl}/api/request/rejectfirstlevelrequest?requestId=${this.currentRequest.id}`)
            .then(response => {
              this.closeDialogProductFulfillment();
              Swal.fire('Approved!', 'Quotation is successfully rejected.', 'success')
                .then(() => {
                  window.location.href = '/request';
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to reject quotation', 'error');
            });
        }
      });
    },
    approveRequest() {
      Swal.fire({
        title: 'Confirmation',
        text: 'Do you want to approve the demo request?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Approve',
        cancelButtonText: 'Cancel',
        allowOutsideClick: true
      }).then((result) => {
        if (result.isConfirmed) {
          const requestUrl = `${this.$config.restUrl}/api/request/approvefinalrequest?requestId=${this.currentRequest.id}&isAccept=true`;

          this.$axios.post(requestUrl)
            .then(response => {
              Swal.fire('Done!', 'Request is successfully approved.', 'success')
                .then(() => {
                  window.location.href = '/request';
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to process the request', 'error');
            });
        } 
      });
    },

    closeDialogProductFulfillment() {
      this.closeDialogProductFulfillment = false;
    },
    fulfillNonCoverplusItem() {
      this.editedItem = { ...this.nonCoverplusRequestItem[0] };
      this.dialogProductFulfillment = true;
    },
    fulfillCoverplusItem() {
      this.editedItem = { ...this.coverplusRequestItem[0] };
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
      newCompetitor.distyPrice = newCompetitor.distyPrice || 0;
      newCompetitor.dealerPrice = newCompetitor.dealerPrice || 0;
      newCompetitor.endUserPrice = newCompetitor.endUserPrice || 0;
      this.competitors.push(newCompetitor);
      this.showAddedCompetitors(newCompetitor);
      this.competitor.brand = null;
      this.competitor.model = null;
      this.competitor.distyPrice = null;
      this.competitor.dealerPrice = null;
      this.competitor.endUserPrice = null;
      this.dialogCompetitor = false;
    },
    removeCompetitorInformation(index) {
      this.competitorsToShow.splice(index, 1);
    },
    showAddedCompetitors(newCompetitor) {
      this.competitorsToShow.push(newCompetitor);
    },
    openAddCoverplusDialog() {
      this.selectedCoverplus = {};
      this.dialogCoverplus = true;
    },
    openEditCoverplusDialog(coverplus) {
      this.selectedCoverplus = { ...coverplus };

      this.$nextTick(() => {
        this.dialogCoverplus = true;
        this.$refs.coverplusDialog.setEditMode(true, coverplus);
      });
    },
    editCoverplusRow(editedCoverplus) {
      const index = this.coverplusesToShow.findIndex(coverplus => coverplus.id === editedCoverplus.id);

      if (index !== -1) {
        const updatedCoverplus = { ...editedCoverplus };
        updatedCoverplus.distyPrice = updatedCoverplus.distyPrice || 0;
        updatedCoverplus.dealerPrice = updatedCoverplus.dealerPrice || 0;
        updatedCoverplus.endUserPrice = updatedCoverplus.endUserPrice || 0;
        updatedCoverplus.warrantyRequest = updatedCoverplus.warrantyRequest || '';
        updatedCoverplus.warrantyRequestPeriod = updatedCoverplus.warrantyRequestPeriod || '';

        this.$set(this.coverplusesToShow, index, updatedCoverplus);
      } else {
        console.error("Product not found for editing.");
      }

      this.dialogProduct = false;
    },
    addCoverplusRow(coverplus) {
      const newCoverplus = { ...coverplus, slaType: 'Local' };
      this.coverpluses.push(newCoverplus);
      this.showAddedCoverpluses(newCoverplus);
      this.coverplus = {
        category: null,
        productId: null,
        quantity: null,
        distyPrice: null,
        dealerPrice: null,
        endUserPrice: null,
        remarks: null,
        warrantyRequest: null,
        warrantyRequestPeriod: null,
        status: 0,
        slaType: 'Local'
      };
      this.dialogCoverplus = false;
    },
    removeCoverplus(index) {
      this.coverplusesToShow.splice(index, 1);
    },
    showUpdatedCoverplus(updatedCoverplus) {
      const index = this.coverplusesToShow.findIndex(coverplus => coverplus.id === updatedCoverplus.id);

      if (index !== -1) {
        this.$set(this.coverplusesToShow, index, updatedCoverplus);
      }
    },
    showAddedCoverpluses(newCoverplus) {
      this.coverplusesToShow.push(newCoverplus);
    },
    removeProduct(index) {
      this.productsToShow.splice(index, 1);
    },
    showAddedProducts(newProduct) {
      this.productsToShow.push(newProduct);
    },
    showUpdatedProducts(updatedProduct) {
      const index = this.productsToShow.findIndex(product => product.id === updatedProduct.id);

      if (index !== -1) {
        this.$set(this.productsToShow, index, updatedProduct);
      } else {
        console.error("Product not found for updating in the display array.");
      }
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
    populateForm(requestData) {
      this.currentRequest = requestData;
      for (const productModel of requestData.requestProducts) {
        const categoryFound = productModel.categoryId
          ? this.categories.find((c) => c.id === productModel.categoryId)
          : null;

        if (categoryFound) {
          this.selectedCategories.push(categoryFound);
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

          const fulfilledDate = new Date(productModel.fulfilledDate);
          fulfilledDate.setHours(fulfilledDate.getHours() + 8);
          const formattedFulfilledDate = moment(fulfilledDate).format('DD/MM/YYYY hh:mm A');

          const p = {
            fulfilledDate: formattedFulfilledDate,
            authorizedToFulfill: productModel.authorizedToFulfill,
            id: productModel.id,
            category: categoryFound,
            productId: productModel.productId,
            quantity: productModel.quantity,
            distyPrice: productModel.distyPrice,
            dealerPrice: productModel.dealerPrice,
            endUserPrice: productModel.endUserPrice,
            productName: productModel.productName,
            remarks: (productModel.remarks === null || productModel.remarks === "null") ? 'N/A' : productModel.remarks,
            status: productModel.status,
            statusStr: statusMapping[productModel.status] || 'Pendings',
            fulfilledPrice: productModel.fulfilledPrice,
            warrantyRequest: productModel.warrantyRequest,
            warrantyRequestPeriod: productModel.warrantyRequestPeriod,
            breached: productModel.breached,
            sla: productModel.sla
          };
          if (productModel.isCoverplus === true) {
            this.coverplusesToShow.push(p);
          } else {
            this.productsToShow.push(p);
          }
        }
      }
      for (const competitorModel of requestData.competitorInformations) {
        const c = {
          model: competitorModel.model,
          brand: competitorModel.brand,
          distyPrice: competitorModel.distyPrice,
          dealerPrice: competitorModel.dealerPrice,
          endUserPrice: competitorModel.endUserPrice,
        };
        this.competitorsToShow.push(c);
      }
      this.approvedByName = requestData.approvedByName;
      this.approvedTime = requestData.approvedTime;
      this.submissionDetail = requestData.requestSubmissionDetail;
      this.projectInformation = requestData.projectInformation;
      this.approvalStateStr = requestData.approvalStateStr;
      this.comments = requestData.comments;

      requestData.projectInformation.projectInformationReasons.forEach((populatedReason) => {
        const reasonInData = this.reasons.find((reason) => reason.text === populatedReason.selectedReason);
        if (reasonInData) {
          reasonInData.isChecked = true;
          reasonInData.additionalText = populatedReason.additionalInfo;
        }

        this.projectInformationReasonsToInsert.push({
          id: populatedReason.Id,
          projectInformationId: populatedReason.projectInformationId,
          selectedReason: populatedReason.selectedReason,
          additionalInfo: populatedReason.additionalText || null
        });
      });
      console.log("awd", this.productsToShow);
    },
    async fetchRequestById(id) {
      try {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/request/getrequestbyid`, {
          params: { id }
        });
        this.unpopulatedRequests = response.data.data;
      } catch (error) {
        console.error(error);
      }
    },
    async fetchCategories() {
      try {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/category/getvalidcategories`);
        this.categories = response.data.data;

      } catch (error) {
        console.error(error);
      }
    },
    handleCheckboxChange(reason) {
      this.$nextTick(() => {
        if (reason.isChecked) {
          this.projectInformationReasonsToInsert.push({
            id: 0,
            projectInformationId: 0,
            selectedReason: reason.text,
            additionalInfo: reason.additionalText || null
          });
        } else {
          const index = this.projectInformationReasonsToInsert.findIndex(r => r.selectedReason === reason.text);
          if (index !== -1) {
            this.projectInformationReasonsToInsert.splice(index, 1);
          }
        }
      });
    },
    async fetchProductsForCategory(category) {
      try {
        const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
        this.$set(this.options, category.id, response.data.data);
      } catch (error) {
        console.error(error);
      }
    },
    async loadDraft() {
      try {
        const response = await this.$axios.get('/api/request/loaddraft');

        if (response && response.data) {
          const draft = response.data;

          try {
            this.selectedCategories = draft.selectedCategories ? JSON.parse(draft.selectedCategories) : [];
            this.productsToShow = draft.productsToShow ? JSON.parse(draft.productsToShow) : [];
            this.competitorsToShow = draft.competitorsToShow ? JSON.parse(draft.competitorsToShow) : [];
            this.coverplusesToShow = draft.coverplusesToShow ? JSON.parse(draft.coverplusesToShow) : [];
            this.submissionDetail = draft.submissionDetail ? JSON.parse(draft.submissionDetail) : {};
            this.projectInformation = draft.projectInformation ? JSON.parse(draft.projectInformation) : {};
            this.reasons = draft.reasons ? JSON.parse(draft.reasons) : [];
            this.priority = draft.priority ? JSON.parse(draft.priority) : {};

            this.comments = draft.comments || '';
            this.customerName = draft.customerName || '';
            this.dealJustification = draft.dealJustification || '';
            this.deadline = draft.deadline || '';


            this.$swal('Success', 'Draft loaded successfully', 'success');
          } catch (error) {
            console.error('Error parsing draft data:', error);
            this.$swal('Error', 'Failed to parse draft data', 'error');
          }
        } else {
          this.$swal('Error', 'Failed to load draft', 'error');
        }
      } catch (error) {
        console.error('Error loading draft:', error);
        if (error.response && error.response.status === 404) {
          this.$swal('Error', 'No draft found for the current user', 'error');
        } else {
          this.$swal('Error', 'Failed to load draft', 'error');
        }
      }
    },
    async saveDraft() {
      try {
        const draftData = {
          SelectedCategories: JSON.stringify(this.selectedCategories),
          ProductsToShow: JSON.stringify(this.productsToShow),
          CompetitorsToShow: JSON.stringify(this.competitorsToShow),
          CoverplusesToShow: JSON.stringify(this.coverplusesToShow),
          SubmissionDetail: JSON.stringify(this.submissionDetail),
          ProjectInformation: JSON.stringify(this.projectInformation),
          Reasons: JSON.stringify(this.reasons),
          Priority: JSON.stringify(this.priority),
          Comments: this.comments,
          CustomerName: this.customerName,
          DealJustification: this.dealJustification,
          Deadline: this.deadline
        };

        const response = await this.$axios.post('/api/request/savedraft', { data: draftData });

        if (response && response.status === 200) {
          this.$swal('Success', response.data.message, 'success');
        } else {
          this.$swal('Error', 'Failed to save draft', 'error');
        }
      } catch (error) {
        console.error('Error saving draft:', error);
        this.$swal('Error', 'Failed to save draft', 'error');
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
      window.location.href = '/request';
    },
    acceptDeal() {
      this.closeDeal(true);
    },
    rejectDeal() {
      this.closeDeal(false);
    },
    exitDeal() {
      try {
        Swal.fire({
          title: 'Are you sure you want to close this deal?',
          showDenyButton: true,
          confirmButtonText: `Proceed`,
          denyButtonText: `Cancel`,
        }).then(async (result) => {
          if (result.isConfirmed) {
            const response = await this.$axios.post(`${this.$config.restUrl}/api/request/exitdeal?id=${this.currentRequest.id}`);
            if (response.status === 200) {
              Swal.fire('Closed deal!', '', 'success')
                .then(() => {
                  window.location.href = '/request';
                });
            }
          } else if (result.isDenied) {
            Swal.fire('Deal not closed', '', 'info')
          }
        })
      } catch (err) {
        console.log(err);
        Swal.fire({
          icon: 'error',
          title: 'Oops...',
          text: err.response ? err.response.data.message : "Failed to close deal!"
        });
      }
    },
    closeDeal(isAccept) {
      try {
        Swal.fire({
          title: 'Are you sure you want to close this deal?',
          showDenyButton: true,
          confirmButtonText: `Proceed`,
          denyButtonText: `Cancel`,
        }).then(async (result) => {
          if (result.isConfirmed) {
            const response = await this.$axios.post(`${this.$config.restUrl}/api/request/closedeal?id=${this.currentRequest.id}&isAccept=${isAccept}`);
            if (response.status === 200) {
              Swal.fire('Closed!', '', 'success')
                .then(() => {
                  window.location.href = '/request';
                });
            }
          } else if (result.isDenied) {
            Swal.fire('Deal not closed', '', 'info')
          }
        })
      } catch (err) {
        console.log(err);
        Swal.fire({
          icon: 'error',
          title: 'Oops...',
          text: err.response ? err.response.data.message : "Failed to close deal!"
        });
      }
    },
    validateForm() {
      const hasReasonChecked = this.reasons.some(reason => reason.isChecked);
      if (!hasReasonChecked) {
        return "At least one reason must be selected!";
      }

      if (this.projectInformation.budget == null || this.projectInformation.budget === "0" || this.projectInformation.budget === "") {
        return "Customer's budget must not be empty!";
      } else if (this.projectInformation.type == null) {
        return "Type must not be empty!";
      } else if (this.projectInformation.requirements == null) {
        return "Customer's requirements must not be empty!";
      } else if (this.coverplusesToShow.length === 0 && this.productsToShow.length === 0) {
        return "Main Unit / Coverplus must not be empty!";
      } else if (this.productsToShow.length > 0 && this.competitorsToShow.length === 0) {
        return "At least one competitor is required!";
      } else if (this.projectInformation.closingDate == null) {
        return "Closing Date must not be empty!";
      } else if (this.projectInformation.deliveryDate == null) {
        return "Delivery Date must not be empty!";
      } else {
        return "";
      }
    },
    exportToExcel() {
      const navBar = document.querySelector('.ums-header');
      const originalDisplayStyle = navBar.style.display;
      navBar.style.display = 'none';

      html2canvas(document.body, {
        x: 0,
        y: navBar.offsetHeight,
        width: document.body.offsetWidth,
        height: document.body.offsetHeight - navBar.offsetHeight,
        useCORS: true
      }).then(canvas => {
        navBar.style.display = originalDisplayStyle;

        const imgData = canvas.toDataURL('image/jpeg', 0.8);
        const pdf = new JsPDF({
          orientation: 'portrait',
          unit: 'px',
          format: [canvas.width, canvas.height],
        });

        pdf.addImage(imgData, 'JPEG', 0, 0, canvas.width, canvas.height);

        const requestId = this.currentRequest.id;
        const fileName = `request_${requestId}.pdf`;

        pdf.save(fileName);

      }).catch(error => {
        navBar.style.display = originalDisplayStyle;
        console.error('Error exporting to PDF:', error);
        Swal.fire('Error', 'Failed to generate PDF file', 'error');
      });
    },
    processQuotation() {
      const quotationData = {
        ApprovalState: 20,
        Priority: this.priority,
        requestProducts: [],
        competitorInformations: [],
        comments: this.comments
      };

      if (this.isMode('editable')) {
        quotationData.id = this.currentRequest.id;
      }

      for (const product in this.productsToShow) {
        const productToInsert = {
          categoryId: this.productsToShow[product].category.id,
          productId: this.productsToShow[product].productId,
          quantity: this.productsToShow[product].quantity,
          distyPrice: this.productsToShow[product].distyPrice,
          dealerPrice: this.productsToShow[product].dealerPrice,
          endUserPrice: this.productsToShow[product].endUserPrice,
          status: this.productsToShow[product].status,
          isCoverplus: false
        };
        quotationData.requestProducts.push(productToInsert);
      }
      for (const competitor in this.competitorsToShow) {
        const competitorToInsert = {
          model: this.competitorsToShow[competitor].model,
          brand: this.competitorsToShow[competitor].brand,
          distyPrice: this.competitorsToShow[competitor].distyPrice,
          dealerPrice: this.competitorsToShow[competitor].dealerPrice,
          endUserPrice: this.competitorsToShow[competitor].endUserPrice
        };
        quotationData.competitorInformations.push(competitorToInsert);
      }
      for (const coverplus in this.coverplusesToShow) {
        const coverplusToInsert = {
          categoryId: this.coverplusesToShow[coverplus].category.id,
          productId: this.coverplusesToShow[coverplus].productId,
          quantity: this.coverplusesToShow[coverplus].quantity,
          distyPrice: this.coverplusesToShow[coverplus].distyPrice,
          dealerPrice: this.coverplusesToShow[coverplus].dealerPrice,
          endUserPrice: this.coverplusesToShow[coverplus].endUserPrice,
          warrantyRequest: this.coverplusesToShow[coverplus].warrantyRequest,
          warrantyRequestPeriod: this.coverplusesToShow[coverplus].warrantyRequestPeriod,
          status: this.coverplusesToShow[coverplus].status,
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
        staggeredMonth: this.projectInformation.staggeredMonth === 'None' ? '' : this.projectInformation.staggeredMonth,
        staggeredComments: this.projectInformation.staggeredComments,
        otherInformation: this.projectInformation.otherInformation,
        projectInformationReasons: this.projectInformationReasonsToInsert
      }

      return quotationData;
    },
    async submitQuotation() {
      this.loading = true;
      let clientErr = "";
      clientErr = this.validateForm();

      if (clientErr) {
        this.$swal(clientErr);
        this.loading = false;
        return;
      }
      if (this.submitting) {
        return;
      }
      this.submitting = true;



      const apiEndpoint = this.isMode('editable') ? `${this.$config.restUrl}/api/request/editrequest` : `${this.$config.restUrl}/api/request/createrequest`;
      const quotationData = this.processQuotation();

      try {
        const vm = this;
        await this.$axios.post(apiEndpoint, {
          data: {
            segment: "string",
            approvalState: 10,
            RequestProducts: quotationData.requestProducts,
            CompetitorInformations: quotationData.competitorInformations,
            requestSubmissionDetail: quotationData.submissionDetail,
            ProjectInformation: quotationData.projectInformation,
            Id: quotationData.id,
            comments: quotationData.comments
          }
        }).then(response => {
          const successMessage = apiEndpoint.endsWith('/editrequest')
            ? 'Request successfully amended'
            : 'Request successfully created';

          this.$swal(successMessage)
            .then((confirm) => {
              if (confirm) {
                window.location.href = '/request';
                localStorage.clear();
              }
            });
        }).catch(err => {
          console.log(err);
          const errorMessage = err.response && err.response.data && err.response.data.message
            ? err.response.data.message
            : 'An unknown error occurred';
          vm.$swal('Failed to submit request', errorMessage, 'error');
        })
      } catch (error) {
        console.log(error);
      } finally {
        this.submitting = false;
        this.loading = false;
      }
    }
  }
}
