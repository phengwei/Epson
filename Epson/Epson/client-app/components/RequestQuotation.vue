<template>
  <div class="create-quotation-container">
    <h1>Pricing Request</h1>

    <v-card class="mx-auto" width="800">
      <v-card-text>
        <!-- Fulfiller Dialog -->
        <ProductFulfillmentDialog :editedItem="editedItem"
                                  :dialogProductFulfillment.sync="dialogProductFulfillment" />
        <div class="d-flex justify-end">
          <v-btn v-if="isMode('isFulfill')"
                 class="mr-2"
                 color="primary"
                 @click="fulfillNonCoverplusItem()">
            Fulfill request
          </v-btn>
          <v-btn v-if="isMode('isFulfillCoverplus')"
                 class="mr-2"
                 color="primary"
                 @click="fulfillCoverplusItem()">
            Fulfill coverplus request
          </v-btn>
          <v-btn v-if="isMode('isApprove') && currentRequestApprovalState === ApprovalStateEnum.PendingSalesSectionHeadAction"
                 class="mr-2"
                 color="primary"
                 @click="approveQuotation()">
            Approve Quotation
          </v-btn>
          <v-btn v-if="isMode('isFinalApprove') && currentRequestApprovalState === ApprovalStateEnum.PendingSalesSectionHeadFinalAction"
                 class="mr-2"
                 color="primary"
                 @click="approveRequest()">
            Approve Request
          </v-btn>
          <v-btn v-if="isMode('amendable')"
                 class="mr-2"
                 color="primary"
                 @click="confirmAmmendQuotation()">
            Set Request to Amend
          </v-btn>
        </div>

        <!-- Product Dialog -->
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
                  <th colspan="11"><h2>PRICE EXPECTATION (RM)</h2></th>
                </tr>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Disty Price</th>
                  <th>Dealer Price</th>
                  <th>End User Price</th>
                  <th v-if="isViewMode">Fulfilled Price</th>
                  <th v-if="isViewMode">Remarks</th>
                  <th v-if="isViewMode">Status</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(product, index) in productsToShow" :key="index">
                  <td>{{ product.category ? product.category.name : 'N/A' }}</td>
                  <td>{{ product.productId ? findProductName(product.productId) : product.productName }}</td>
                  <td>{{ product.quantity || 'N/A' }}</td>
                  <td>{{ product.distyPrice || 'N/A' }}</td>
                  <td>{{ product.dealerPrice || 'N/A' }}</td>
                  <td>{{ product.endUserPrice || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ product.fulfilledPrice || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ product.remarks || 'N/A' }}</td>
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
        <!-- Coverplus Dialog -->
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
                  <th colspan="7"><h2>PRICE EXPECTATION (RM)</h2></th>
                </tr>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Disty Price</th>
                  <th>Dealer Price</th>
                  <th>End User Price</th>
                  <th v-if="isViewMode">Fulfilled Price</th>
                  <th v-if="isViewMode">Remarks</th>
                  <th v-if="isViewMode">Status</th>
                  <th v-if="!isViewMode">Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(coverplus, index) in coverplusesToShow" :key="index">
                  <td>{{ coverplus.category ? coverplus.category.name : 'N/A' }}</td>
                  <td>{{ coverplus.productId ? findProductName(coverplus.productId) : coverplus.productName }}</td>
                  <td>{{ coverplus.quantity || 'N/A' }}</td>
                  <td>{{ coverplus.distyPrice || 'N/A' }}</td>
                  <td>{{ coverplus.dealerPrice || 'N/A' }}</td>
                  <td>{{ coverplus.endUserPrice || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ coverplus.fulfilledPrice || 'N/A' }}</td>
                  <td v-if="isViewMode">{{ coverplus.remarks || 'N/A' }}</td>
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
        <!-- Competitor Information Dialog -->
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
                  <th colspan="6"><h2>COMPETITOR'S INFORMATION</h2></th>
                </tr>
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
            <table class="mb-5 mt-2">
              <thead>
                <tr class="header-row">
                  <th colspan="4"><h2>SUBMISSION DETAILS</h2></th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Prepared By (EMSB)</td>
                  <td>:</td>
                  <td><input type="text" v-model="submissionDetail.preparedBy" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Request Date</td>
                  <td>:</td>
                  <td><input type="datetime-local" v-model="submissionDetail.createdOnUTC" class="border-input" readonly></td>
                </tr>
                <tr>
                  <td>Distributor Name</td>
                  <td>:</td>
                  <td>
                    <select v-model="submissionDetail.distributorName" class="border-input" :class="{'readonly-field': isViewMode}" :disabled="isViewMode">
                      <option v-for="distributor in distributors" :key="distributor" :value="distributor">
                        {{ distributor }}
                      </option>
                    </select>
                  </td>
                </tr>
                <tr>
                  <td>BP / SI / Reseller Name</td>
                  <td>:</td>
                  <td><input type="text" v-model="submissionDetail.resellerName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Contact Person Name</td>
                  <td>:</td>
                  <td><input type="text" v-model="submissionDetail.contactPersonName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Telephone No</td>
                  <td>:</td>
                  <td><input type="text" v-model="submissionDetail.telephoneNo" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Fax No</td>
                  <td>:</td>
                  <td><input type="text" v-model="submissionDetail.faxNo" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Email</td>
                  <td>:</td>
                  <td><input type="text" v-model="submissionDetail.email" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <v-card class="mb-5 mt-2">
          <v-card-text>
            <table class="mb-5 mt-2">
              <thead>
                <tr class="header-row">
                  <th colspan="6"><h2>END USER / PROJECT INFORMATION</h2></th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>Company / Project Name</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.projectName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Project ID</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.projectId" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Industry</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.industry" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Type</td>
                  <td>:</td>
                  <td>
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
                  <td>Closing Date</td>
                  <td>:</td>
                  <td><input type="datetime-local" v-model="projectInformation.closingDate" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Delivery Date</td>
                  <td>:</td>
                  <td><input type="datetime-local" v-model="projectInformation.deliveryDate" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Company Address</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.companyAddress" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Contact Person</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.contactPersonName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Telephone No</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.telephoneNo" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Email</td>
                  <td>:</td>
                  <td><input type="text" v-model="projectInformation.email" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
                </tr>
                <tr>
                  <td>Reason</td>
                  <td>:</td>
                  <td>
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
              <td>Key Customer Requirements</td>
              <td>:</td>
              <td><input type="text" v-model="projectInformation.requirements" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
              </tr>
              <tr>
                <td>Customer Applications</td>
                <td>:</td>
                <td><input type="text" v-model="projectInformation.customerApplications" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
              </tr>
              <tr>
                <td>Customer's Budget for this purchase'</td>
                <td>:</td>
                <td><input type="number" min="1" v-model="projectInformation.budget" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
              </tr>
              <tr>
                <td>If Staggered Delivery, please state deliver qty & timeline</td>
                <td>:</td>
                <td><input type="text" v-model="projectInformation.staggeredDelivery" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
              </tr>
              <tr>
                <td>Other Information</td>
                <td>:</td>
                <td><input type="text" v-model="projectInformation.otherInformation" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></td>
              </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>
        <div class="form-group" v-if="comments != ''">
          <label>Comments</label>
          <textarea v-model="comments" class="border-input" :readonly="isCommentEditable"></textarea>
        </div>
        <button type="submit" @click="submitQuotation" v-if="isMode('create')">Submit</button>
        <button type="submit" @click="saveDraft" v-if="isMode('create')">Save Draft</button>
        <button type="submit" @click="submitQuotation" v-if="isMode('editable')">Amend Request</button>
        <button type="submit" @click="acceptDeal" v-if="isMode('dealable')">Accept Deal</button>
        <button type="submit" @click="rejectDeal" v-if="isMode('dealable')">Reject Deal</button>
        <button type="submit" @click="exitDeal" v-if="isMode('amendable')">Close Deal</button>
        <button type="submit" @click="redirectToRequest" v-if="loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Sales') || loggedInUser.roles.includes('Sales Section Head')">
          Return to Request
        </button>
      </v-card-text>
    </v-card>

  </div>
</template>

<script>
  import requestQuotation from '~/script/requestQuotation.js';
  export default {
    ...requestQuotation,
    name: "request-quotation",
  }
</script>

<style scoped>
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
