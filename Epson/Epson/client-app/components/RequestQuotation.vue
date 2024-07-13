<template>
  <div class="create-quotation-container">
    <v-overlay :value="loading" class="loading-overlay">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>

    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;" v-if="!loading">
      <v-toolbar flat>
        <v-toolbar-title><h1 class="blue-text big-bold">PRICING REQUEST</h1></v-toolbar-title>
      </v-toolbar>
      <v-card-text>
        <!-- Fulfiller Dialog -->
        <ProductFulfillmentDialog :editedItem="editedItem" :dialogProductFulfillment.sync="dialogProductFulfillment" />

        <!-- Product Dialog -->
        <ProductDialog ref="productDialog" :dialogProduct.sync="dialogProduct"
                       :product="product"
                       :isViewMode="isViewMode"
                       @add-product="addProductRow"
                       @edit-product="editProductRow" />
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions filter-container mb-4 d-flex align-center justify-space-between">
              <span class="blue-text small-bold">MAIN UNIT</span>
              <v-btn v-if="!isViewMode" color="primary" @click="openAddProductDialog">
                Add New
              </v-btn>
            </div>
            <table class="mb-5 mt-2">
              <thead>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Disty Price</th>
                  <th>Dealer Price</th>
                  <th>End User Price</th>
                  <th v-if="isViewMode">Remarks</th>
                  <th v-if="isViewMode">Status</th>
                  <th v-if="isFulfillMode">Fulfill</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(product, index) in productsToShow" :key="index"
                    :class="{
                      'highlighted': currentRequestApprovalState === ApprovalStateEnum.Approved && product.breached,
                      'not-approved': isFulfillMode && product.statusStr !== 'Approved',
                      'approved': isAmendMode && product.statusStr === 'Approved'
                    }">
                  <td>{{ product.category ? product.category.name : 'N/A' }}</td>
                  <td>{{ product.productId ? findProductName(product.productId) : product.productName }}</td>
                  <td>{{ product.quantity || 'N/A' }}</td>
                  <td>{{ product.distyPrice || 'N/A' }}</td>
                  <td>{{ product.dealerPrice || 'N/A' }}</td>
                  <td>{{ product.endUserPrice || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ product.remarks || 'N/A' }}</td>
                  <td v-if="isViewMode">
                    <span v-if="product.statusStr === 'Approved'">
                      Approved at RM {{ product.dealerPrice }}
                    </span>
                    <span v-else>
                      {{ product.statusStr || 'N/A' }}
                    </span>
                  </td>
                  <td v-if="isFulfillMode">
                    <v-btn v-if="product.authorizedToFulfill === true" small color="primary" @click="openFulfillProductDialog(product)">
                      <v-icon>mdi-pencil</v-icon>
                    </v-btn>
                  </td>
                  <td v-if="!isViewMode">
                    <v-btn small color="primary" @click="openEditProductDialog(product)">
                      <v-icon>mdi-pencil</v-icon>
                    </v-btn>
                    <v-btn small color="error" @click="removeProduct(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <!-- Coverplus Dialog -->
        <CoverplusDialog ref="coverplusDialog" :dialogCoverplus.sync="dialogCoverplus"
                         :coverplus="coverplus"
                         :isViewMode="isViewMode"
                         @add-coverplus="addCoverplusRow"
                         @edit-coverplus="editCoverplusRow" />
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions filter-container mb-4 d-flex align-center justify-space-between">
              <span class="blue-text small-bold">COVERPLUS</span>
              <v-btn v-if="!isViewMode" color="primary" @click="openAddCoverplusDialog">
                Add New
              </v-btn>
            </div>
            <table class="mb-5 mt-2">
              <thead>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Warranty Details</th>
                  <th>Disty Price</th>
                  <th>Dealer Price</th>
                  <th>End User Price</th>
                  <th v-if="isViewMode">Remarks</th>
                  <th v-if="isViewMode">Status</th>
                  <th v-if="isFulfillMode">Fulfill</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(coverplus, index) in coverplusesToShow" :key="index"
                    :class="{
                            'highlighted': currentRequestApprovalState === ApprovalStateEnum.Approved && coverplus.breached,
                            'not-approved': isFulfillMode && coverplus.statusStr !== 'Approved',
                            'approved': isAmendMode && coverplus.statusStr === 'Approved'
                          }">
                  <td>{{ coverplus.category ? coverplus.category.name : 'N/A' }}</td>
                  <td>{{ coverplus.productId ? findProductName(coverplus.productId) : coverplus.productName }}</td>
                  <td>{{ coverplus.quantity || 'N/A' }}</td>
                  <td>
                    {{ coverplus.warrantyRequest || 'N/A' }}
                    <span v-if="coverplus.warrantyRequest"> {{ coverplus.warrantyRequestPeriod || "" }}</span>
                  </td>
                  <td>{{ coverplus.distyPrice || 'N/A' }}</td>
                  <td>{{ coverplus.dealerPrice || 'N/A' }}</td>
                  <td>{{ coverplus.endUserPrice || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ coverplus.remarks || 'N/A' }}</td>
                  <td v-if="isViewMode">
                    <span v-if="coverplus.statusStr === 'Approved'">
                      Approved at RM {{ coverplus.dealerPrice }}
                    </span>
                    <span v-else>
                      {{ coverplus.statusStr || 'N/A' }}
                    </span>
                  </td>
                  <td v-if="isFulfillMode">
                    <v-btn v-if="product.authorizedToFulfill === true" small color="primary" @click="openFulfillProductDialog(product)">
                      <v-icon>mdi-pencil</v-icon>
                    </v-btn>
                  </td>
                  <td v-if="!isViewMode">
                    <v-btn small color="primary" @click="openEditCoverplusDialog(coverplus)">
                      <v-icon>mdi-pencil</v-icon>
                    </v-btn>
                    <v-btn small color="error" @click="removeCoverplus(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <!-- Competitor Information Dialog -->
        <CompetitorInformationDialog :dialogCompetitor.sync="dialogCompetitor"
                                     :competitor="competitor"
                                     :isViewMode="isViewMode"
                                     @add-competitor="addCompetitorRow" />
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions filter-container mb-4 d-flex align-center justify-space-between">
              <span class="blue-text small-bold">COMPETITOR INFORMATION</span>
              <v-btn v-if="!isViewMode" color="primary" @click="dialogCompetitor = true">
                Add New
              </v-btn>
            </div>
            <table class="mb-5 mt-2">
              <thead>
                <tr>
                  <th>Model</th>
                  <th>Brand</th>
                  <th>Disty Price</th>
                  <th>Dealer Price</th>
                  <th>End User Price</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(competitor, index) in competitorsToShow" :key="index">
                  <td>{{ competitor.model }}</td>
                  <td>{{ competitor.brand }}</td>
                  <td>{{ competitor.distyPrice || 'N/A' }}</td>
                  <td>{{ competitor.dealerPrice  || 'N/A' }}</td>
                  <td>{{ competitor.endUserPrice || 'N/A' }}</td>
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
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions filter-container mb-4 d-flex align-center justify-space-between">
              <span class="blue-text small-bold">SUBMISSION DETAILS</span>
            </div>
            <table class="mb-5 mt-2">
              <tbody>
                <tr>
                  <td class="td-header">Prepared By (EMSB)</td>
                  <td><input type="text" v-model="submissionDetail.createdByStr" class="border-input" readonly></td>
                </tr>
                <tr>
                  <td class="td-header">Request Date</td>
                  <td>
                    <input v-if="isViewMode"
                           type="text"
                           v-model="formattedRequestDate"
                           class="border-input"
                           readonly />
                    <input v-else
                           type="datetime-local"
                           v-model="submissionDetail.createdOnUTC"
                           class="border-input" readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Distributor Name</td>
                  <td>
                    <select v-model="submissionDetail.distributorName" class="border-input" :class="{'readonly-field': isViewMode}" :disabled="isViewMode">
                      <option v-for="distributor in distributors" :key="distributor" :value="distributor">
                        {{ distributor }}
                      </option>
                    </select>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">BP / SI / Reseller Name</td>
                  <td><input type="text" v-model="submissionDetail.resellerName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Contact Person Name</td>
                  <td><input type="text" v-model="submissionDetail.contactPersonName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Telephone No <span class="required-asterisk">*</span></td>
                  <td><input type="text" v-model="submissionDetail.telephoneNo" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Fax No</td>
                  <td><input type="text" v-model="submissionDetail.faxNo" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Email <span class="required-asterisk">*</span></td>
                  <td><input type="text" v-model="submissionDetail.email" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr v-if="currentRequestApprovalState === ApprovalStateEnum.Approved">
                  <td class="td-header">Approved By</td>
                  <td>
                    <input v-if="isViewMode"
                           type="text"
                           v-model="this.approvedByName"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr v-if="currentRequestApprovalState === ApprovalStateEnum.Approved">
                  <td class="td-header">Approved Time</td>
                  <td>
                    <input v-if="isViewMode"
                           type="text"
                           v-model="formattedApprovedTime"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <div class="table-actions filter-container mb-4 d-flex align-center justify-space-between">
              <span class="blue-text small-bold">END USER / PROJECT INFORMATION</span>
            </div>
            <table class="mb-5 mt-2">
              <tbody>
                <tr>
                  <td class="td-header">Company / Project Name</td>
                  <td><input type="text" v-model="projectInformation.projectName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode" required></td>
                </tr>
                <tr>
                  <td class="td-header">Project ID</td>
                  <td><input type="text" v-model="projectInformation.projectId" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode" required></td>
                </tr>
                <tr>
                  <td class="td-header">Industry</td>
                  <td><input type="text" v-model="projectInformation.industry" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode" required></td>
                </tr>
                <tr>
                  <td class="td-header">Company Address</td>
                  <td><input type="text" v-model="projectInformation.companyAddress" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Contact Person</td>
                  <td><input type="text" v-model="projectInformation.contactPersonName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Telephone No <span class="required-asterisk">*</span></td>
                  <td><input type="text" v-model="projectInformation.telephoneNo" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Email</td>
                  <td><input type="text" v-model="projectInformation.email" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Type <span class="required-asterisk">*</span></td>
                  <td class="td-content">
                    <div class="form-check">
                      <input class="form-check-input custom-radio" type="radio" id="openTender" value="Open Tender" v-model="projectInformation.type" :disabled="isViewMode">
                      <label class="form-check-label" for="openTender">Open Tender</label>
                    </div>
                    <div class="form-check">
                      <input class="form-check-input custom-radio" type="radio" id="closeTender" value="Close Tender" v-model="projectInformation.type" :disabled="isViewMode">
                      <label class="form-check-label" for="closeTender">Close Tender</label>
                    </div>
                    <div class="form-check">
                      <input class="form-check-input custom-radio" type="radio" id="specialPricing" value="Special Pricing" v-model="projectInformation.type" :disabled="isViewMode">
                      <label class="form-check-label" for="specialPricing">Special Pricing</label>
                    </div>
                  </td>
                </tr>
                <tr>
                  <td class="td-header reason-header">Reason</td>
                  <td class="td-content">
                    <div class="form-group" v-for="(reason, index) in reasons" :key="index">
                      <div class="form-check">
                        <input class="form-check-input custom-checkbox" type="checkbox" :id="reason.text" :value="reason.text" v-model="reason.isChecked" :disabled="isViewMode" @change="handleCheckboxChange(reason)">
                        <label class="form-check-label" :for="reason.text">{{ reason.text }}</label>
                      </div>
                      <div v-if="reason.isChecked && (reason.text === 'Additional Purchase' || reason.text === 'Renewal of Quotation' || reason.text === 'Revision (Price/Model/Qty/Other)')">
                        <input type="text" v-model="reason.additionalText" placeholder="Enter Quotation No." class="border-input mt-2" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
                      </div>
                    </div>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Closing Date <span class="required-asterisk">*</span></td>
                  <td>
                    <input v-if="isViewMode"
                           type="text"
                           v-model="formattedClosingDate"
                           class="border-input"
                           readonly />
                    <input v-else
                           type="datetime-local"
                           v-model="projectInformation.closingDate"
                           class="border-input" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Delivery Date <span class="required-asterisk">*</span></td>
                  <td>
                    <input v-if="isViewMode"
                           type="text"
                           v-model="formattedDeliveryDate"
                           class="border-input"
                           readonly />
                    <input v-else
                           type="datetime-local"
                           v-model="projectInformation.deliveryDate"
                           class="border-input" />
                  </td>
                </tr>

                <tr>
                  <td class="td-header">If Staggered Delivery, please select the month</td>
                  <td>
                    <select v-model="projectInformation.staggeredMonth" class="border-input" :class="{'readonly-field': isViewMode}" :disabled="isViewMode">
                      <option v-for="month in months" :key="month" class="form-check-label" :value="month">{{ month }}</option>
                    </select>
                    <div class="mt-2" v-if="projectInformation.staggeredMonth && projectInformation.staggeredMonth != 'None'">
                      <input type="text" v-model="projectInformation.staggeredComments" placeholder="Enter Reason" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
                    </div>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Key Customer Requirements <span class="required-asterisk">*</span></td>
                  <td><input type="text" v-model="projectInformation.requirements" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Customer Applications</td>
                  <td><input type="text" v-model="projectInformation.customerApplications" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Customer's Budget for this purchase <span class="required-asterisk">*</span></td>
                  <td><input type="number" min="1" v-model="projectInformation.budget" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td class="td-header">Other Information</td>
                  <td><input type="text" v-model="projectInformation.otherInformation" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <button type="submit" @click="submitQuotation" v-if="isMode('create')">Submit</button>
        <button type="submit" @click="saveDraft" v-if="isMode('create')">Save Draft</button>
        <button type="submit" @click="saveDraft" v-if="loggedInUser.roles.includes('Sales') && !isMode('create')">Copy Form</button>
        <button type="submit" @click="loadDraft" v-if="isMode('create')">Load Form</button>
        <button type="submit" @click="submitQuotation" v-if="isMode('editable')">Amend Request</button>
        <button type="submit" @click="acceptDeal" v-if="isMode('dealable')">Accept Deal</button>
        <button type="submit" @click="rejectDeal" v-if="isMode('dealable')">Reject Deal</button>
        <button type="submit" @click="exitDeal" v-if="isMode('amendable')">Exit Deal</button>
        <button type="submit" @click="approveRequest" v-if="isMode('isFinalApprove') && currentRequestApprovalState === ApprovalStateEnum.PendingSalesSectionHeadFinalAction">Approve Request</button>
        <button type="submit" @click="confirmAmmendQuotation" v-if="isMode('amendable')">Set Request to Amend</button>
        <button type="submit" @click="approveQuotation()" v-if="isMode('isApprove') && currentRequestApprovalState === ApprovalStateEnum.PendingSalesSectionHeadAction">Approve Quotation</button>
        <button type="submit" @click="rejectQuotation()" v-if="isMode('isApprove') && currentRequestApprovalState === ApprovalStateEnum.PendingSalesSectionHeadAction">Reject Quotation</button>
        <button type="submit" @click="redirectToRequest" v-if="loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Sales') || loggedInUser.roles.includes('Sales Section Head')">
          Return to request list
        </button>
        <button type="submit" @click="exportToExcel" v-if="isMode('view')">
          Export
        </button>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import requestQuotation from '~/script/requestQuotation.js';
  export default {
    ...requestQuotation,
    name: "request-quotation"
  }
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';

  .not-approved {
    border-left: 4px solid red;
    background-color: #ffcccc;
  }

  .approved {
    border-left: 4px solid green;
    background-color: #90EE90;
  }

  .highlighted {
    background-color: #ffcccc;
  }

  .products-title {
    font-size: 2em;
    text-align: center;
  }

  .custom-radio {
    border: 1px solid #000;
    margin: 2px;
    width: 1.2em;
    height: 1.2em;
    border-radius: 50%;
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

  th {
    background-color: #003399;
    color: white;
  }

  .td-header {
    background-color: #003399;
    color: white;
    width: 20%;
  }

  .reason-header {
    font-weight: bold;
    padding: 10px;
    vertical-align: top;
    border-right: 1px solid white; /* Add right border */
  }

  .td-content {
    padding: 10px;
    width: 80%;
    border-left: 1px solid #ddd; /* Add left border */
  }

  .required-asterisk {
    color: red;
    margin-left: 2px;
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

  .filter-container {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }
  .loading-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    z-index: 9999;
    display: flex;
    align-items: center;
    justify-content: center;
  }

</style>
