import { mapGetters } from 'vuex';
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
      categories: [],
      selectedCategories: [],
      isChecked: [],
      selectedProducts: {},
      months: ['None', 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
      product: { category: null, productId: null, quantity: null, distyPrice: null, dealerPrice: null, endUserPrice: null, remarks: null },
      products: [],
      coverplus: { category: null, productId: null, quantity: null, distyPrice: null, dealerPrice: null, endUserPrice: null },
      coverpluses: [],
      competitor: { model: null, brand: null, distyPrice: null, dealerPrice: null, endUserPrice: null },
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
        { text: "New Purchase", info: "", isChecked: false, additionalText: "" },
        { text: "Additional Purchase", info: "Quotation No.", isChecked: false, additionalText: "" },
        { text: "Renewal of Quotation", info: "Quotation No.", isChecked: false, additionalText: "" },
        { text: "Revision (Price/Model/Qty/Other)", info: "Quotation No.", isChecked: false, additionalText: "" },
        { text: "Replacement of old machine", info: "", isChecked: false, additionalText: "" }
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
      distributors: ['Servex', 'Ingram', 'VSTech', 'Etech IT', 'GOS', 'EDAP'],
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
      submitting: false,
    };
  },
  async created() {
    this.submissionDetail.createdOnUTC = this.getToday();
    this.submissionDetail.preparedBy = this.loggedInUser.userName;
    await this.fetchCategories();
    if (this.$route.query.view || this.$route.query.editable) {
      const request = JSON.parse(this.$route.query.request);
      this.populateForm(request);
    }
    else {
      await this.loadDraft();
    }
  },
  computed: {
    ...mapGetters(['isAuthenticated', 'loggedInUser']),
    isCommentEditable() {
      return this.isViewMode && !this.isMode('dealable');
    },
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
                  this.$router.push('/request');
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
                  this.$router.push('/request');
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to approve quotation', 'error');
            });
        }
      });
    },
    approveRequest() {
      Swal.fire({
        title: 'Confirmation',
        text: 'Do you want to approve the request?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Approve',
        cancelButtonText: 'Reject',
        allowOutsideClick: true 
      }).then((result) => {
        if (result.isConfirmed) {
          const requestUrl = `${this.$config.restUrl}/api/request/approvefinalrequest?requestId=${this.currentRequest.id}&isAccept=true`;

          this.$axios.post(requestUrl)
            .then(response => {
              Swal.fire('Done!', 'Request is successfully approved.', 'success')
                .then(() => {
                  this.$router.push('/request');
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to process the request', 'error');
            });
            
        } else if (result.isDismissed && result.dismiss === 'cancel') {
          const requestUrl = `${this.$config.restUrl}/api/request/approvefinalrequest?requestId=${this.currentRequest.id}&isAccept=false`;

          this.$axios.post(requestUrl)
            .then(response => {
              Swal.fire('Done!', 'Request is successfully rejected.', 'success')
                .then(() => {
                  this.$router.push('/request');
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
    addCoverplusRow(coverplus) {
      const newCoverplus = { ...coverplus };
      newCoverplus.distyPrice = newCoverplus.distyPrice || 0;
      newCoverplus.dealerPrice = newCoverplus.dealerPrice || 0;
      newCoverplus.endUserPrice = newCoverplus.endUserPrice || 0;
      this.coverpluses.push(newCoverplus);
      this.showAddedCoverpluses(newCoverplus);
      this.product.category = null;
      this.product.productId = null;
      this.product.quantity = null;
      this.product.distyPrice = null;
      this.product.dealerPrice = null;
      this.product.endUserPrice = null;
      this.dialogCoverplus = false;
    },
    removeCoverplus(index) {
      this.coverplusesToShow.splice(index, 1);
    },
    showAddedCoverpluses(newCoverplus) {
      this.coverplusesToShow.push(newCoverplus);
    },
    addProductRow(product) {
      const newProduct = { ...product };
      newProduct.distyPrice = newProduct.distyPrice || 0;
      newProduct.dealerPrice = newProduct.dealerPrice || 0;
      newProduct.endUserPrice = newProduct.endUserPrice || 0;
      this.products.push(newProduct);
      this.showAddedProducts(newProduct);
      this.product.category = null;
      this.product.productId = null;
      this.product.quantity = null;
      this.product.distyPrice = null;
      this.product.dealerPrice = null;
      this.product.endUserPrice = null;
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
            productId: productModel.productId,
            quantity: productModel.quantity,
            distyPrice: productModel.distyPrice,
            dealerPrice: productModel.dealerPrice,
            endUserPrice: productModel.endUserPrice,
            productName: productModel.productName,
            remarks: productModel.remarks || 'n/a',
            statusStr: productModel.statusStr,
            fulfilledPrice: productModel.fulfilledPrice
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
          distyPrice: competitorModel.distyPrice,
          dealerPrice: competitorModel.dealerPrice,
          endUserPrice: competitorModel.endUserPrice,
        };
        this.competitorsToShow.push(c);
      }
      this.submissionDetail = requestData.requestSubmissionDetailModel;
      this.projectInformation = requestData.projectInformationModel;
      this.approvalStateStr = requestData.approvalStateStr;
      this.comments = requestData.comments;

      requestData.projectInformationModel.projectInformationReasons.forEach((populatedReason) => {
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
                  this.$router.push('/request');
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
                  this.$router.push('/request');
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
      const emailRegex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;

      const phoneRegex = /^[\d-]{8,12}$/;

      if (this.projectInformation.budget == null || this.projectInformation.budget === "0" || this.projectInformation.budget === "") {
        return "Customer's budget must not be empty!";
      } else if (this.projectInformation.type == null) {
        return "Type must not be empty!";
      } else if (this.projectInformation.requirements == null) {
        return "Customer's requirements must not be empty!";
      } else if (this.productsToShow.length > 0 && this.competitorsToShow.length === 0) {
        return "At least one competitor is required!";
      } else if (!emailRegex.test(this.submissionDetail.email) || !emailRegex.test(this.projectInformation.email)) {
        return "Invalid email format!";
      } else if (!phoneRegex.test(this.submissionDetail.telephoneNo) || !phoneRegex.test(this.projectInformation.telephoneNo) || !phoneRegex.test(this.submissionDetail.faxNo)) {
        return "Invalid phone / fax no. format!";
      } else if (this.projectInformation.closingDate == null) {
          return "Closing Date must not be empty!";
      } else if (this.projectInformation.deliveryDate == null) {
          return "Delivery Date must not be empty!";
      } else {
        return "";
      }
    },
    exportToExcel() {
      this.$axios.get('/api/export/toExcel', {
        params: { requestId: this.projectInformation.requestId },
        responseType: 'blob'
      })
        .then(response => {
          const blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
          const url = URL.createObjectURL(blob);

          Swal.fire({
            title: 'Exported!',
            html: `<a href="${url}" download="request.xlsx">Click here to download</a>`,
            confirmButtonText: 'Close',
            onClose: () => {
              URL.revokeObjectURL(url);
            }
          });
        })
        .catch(error => {
          console.error('Error exporting to Excel:', error);
          Swal.fire('Error', 'Failed to generate Excel file', 'error');
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
          productId: this.productsToShow[product].productId,
          quantity: this.productsToShow[product].quantity,
          distyPrice: this.productsToShow[product].distyPrice,
          dealerPrice: this.productsToShow[product].dealerPrice,
          endUserPrice: this.productsToShow[product].endUserPrice,
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
        staggeredMonth: this.projectInformation.staggeredMonth === 'None' ? '' : this.projectInformation.staggeredMonth,
        staggeredComments: this.projectInformation.staggeredComments,
        otherInformation: this.projectInformation.otherInformation,
        projectInformationReasons: this.projectInformationReasonsToInsert
      }

      return quotationData;
    },
    async submitQuotation() {
      let clientErr = "";
      clientErr = this.validateForm();

      if (clientErr) {
          this.$swal(clientErr);
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
            ProjectInformationModel: quotationData.projectInformation,
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
                this.$router.push('/request');
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
      }
    }
  }
}
