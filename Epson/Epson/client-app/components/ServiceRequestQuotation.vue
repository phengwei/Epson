<template>
  <div class="create-service-request-container">
    <!-- Overlay & Loading Spinner -->
    <v-overlay :value="loading" class="loading-overlay">
      <v-progress-circular indeterminate size="64"></v-progress-circular>
    </v-overlay>

    <v-card class="mx-auto card-round"
            style="width: 90%; padding: 20px;"
            v-if="!loading">
      <!-- Title / Toolbar -->
      <v-toolbar flat>
        <v-toolbar-title>
          <h1 class="blue-text big-bold">Service Request</h1>
        </v-toolbar-title>
      </v-toolbar>

      <v-card-text>
        <!-- BASIC DETAILS -->
        <v-card class="mb-5 mt-2">
          <v-card-title>
            <span class="blue-text small-bold">BASIC DETAILS</span>
          </v-card-title>
          <v-card-text>
            <table>
              <tbody>
                <tr>
                  <td class="td-header">Service Request #</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.serviceRequestNo"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Owner</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.owner"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Owner Group</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.ownerGroup"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Status</td>
                  <td>
                    <select v-model="serviceRequest.status"
                            class="border-input"
                            disabled>
                      <option value="NEW">NEW</option>
                      <option value="INPROG">INPROG</option>
                      <option value="RESOLVED">RESOLVED</option>
                    </select>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Status Date</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.statusDate"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Fix Status</td>
                  <td>
                    <select v-model="serviceRequest.fixStatus"
                            class="border-input"
                            :disabled="!isMakerMode">
                      <option value="">-- Select --</option>
                      <option value="PERMANENTFIX">PERMANENTFIX</option>
                      <option value="TEMPORARYFIX">TEMPORARYFIX</option>
                    </select>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Fix Status Date</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.fixStatusDate"
                           class="border-input"
                           :readonly="!isMakerMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Payment Status</td>
                  <td>
                    <select v-model="serviceRequest.paymentStatus"
                            class="border-input"
                            :disabled="!isCheckerMode">
                      <option value="">-- Select --</option>
                      <option value="PAID">PAID</option>
                      <option value="UNPAID">UNPAID</option>
                    </select>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Payment Status Date</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.paymentStatusDate"
                           class="border-input"
                           :readonly="!isCheckerMode" />
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>

        <!-- USER INFORMATION -->
        <v-card class="mb-5 mt-2">
          <v-card-title>
            <span class="blue-text small-bold">USER INFORMATION</span>
          </v-card-title>
          <v-card-text>
            <!-- Requester / Affected Person -->
            <table>
              <tbody>
                <tr>
                  <td class="td-header">Requester / Affected Person</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.requester"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Name</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.requesterName"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Phone</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.requesterPhone"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">E-mail</td>
                  <td>
                    <input type="email"
                           v-model="serviceRequest.requesterEmail"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
              </tbody>
            </table>

            <hr />

            <!-- Reported By -->
            <table>
              <tbody>
                <tr>
                  <td class="td-header">Reported By</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.reportedBy"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Name</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.reportedByName"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Phone</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.reportedByPhone"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">E-mail</td>
                  <td>
                    <input type="email"
                           v-model="serviceRequest.reportedByEmail"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>

        <!-- SERVICE REQUEST DETAILS -->
        <v-card class="mb-5 mt-2">
          <v-card-title>
            <span class="blue-text small-bold">SERVICE REQUEST DETAILS</span>
          </v-card-title>
          <v-card-text>
            <table>
              <tbody>
                <tr>
                  <td class="td-header">Classification Path</td>
                  <td>
                    <select v-model="serviceRequest.classificationPath"
                            class="border-input"
                            @change="onClassificationChange"
                            :disabled="isViewMode">
                      <option value="">-- Select Classification --</option>
                      <option v-for="(cObj, idx) in classificationData"
                              :key="idx"
                              :value="cObj.name">
                        {{ cObj.name }}
                      </option>
                    </select>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Services</td>
                  <td>
                    <template v-if="isViewMode">
                      <input type="text"
                             v-model="serviceRequest.services"
                             class="border-input"
                             :readonly="isViewMode" />
                    </template>
                    <template v-else>
                      <select v-model="serviceRequest.services"
                              class="border-input"
                              @change="onServiceChange"
                              :disabled="!availableServices.length">
                        <option value="">-- Select Service --</option>
                        <option v-for="(svc, sidx) in availableServices"
                                :key="sidx"
                                :value="svc.name">
                          {{ svc.name }}
                        </option>
                      </select>
                    </template>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Category</td>
                  <td>
                    <template v-if="isViewMode">
                      <input type="text"
                             v-model="serviceRequest.category"
                             class="border-input"
                             :readonly="isViewMode" />
                    </template>
                    <template v-else>
                      <select v-model="serviceRequest.category"
                              class="border-input"
                              @change="onCategoryChange"
                              :disabled="!availableCategories.length">
                        <option value="">-- Select Category --</option>
                        <option v-for="(cat, cidx) in availableCategories"
                                :key="cidx"
                                :value="cat.name">
                          {{ cat.name }}
                        </option>
                      </select>
                    </template>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Section / Unit In Charge</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.section"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Time Tracking (TAT)</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.timeTracking"
                           class="border-input"
                           readonly />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Summary</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.summary"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Details / Description</td>
                  <td>
                    <textarea v-model="serviceRequest.details"
                              class="border-input"
                              rows="5"
                              :readonly="isViewMode"></textarea>
                  </td>
                </tr>

                <!-- NEW FIELDS: Work Notes & Verification Notes (READ-ONLY) -->
                <tr>
                  <td class="td-header">Work Notes</td>
                  <td>
                    <textarea v-model="serviceRequest.workNotes"
                              class="border-input"
                              rows="3"
                              :readonly="!isMakerMode"></textarea>
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Verification Notes</td>
                  <td>
                    <textarea v-model="serviceRequest.verificationNotes"
                              class="border-input"
                              rows="3"
                              :readonly="isCheckerMode"></textarea>
                  </td>
                </tr>
                <!-- END NEW FIELDS -->
              </tbody>
            </table>
          </v-card-text>
        </v-card>

        <!-- DATES -->
        <v-card class="mb-5 mt-2">
          <v-card-title>
            <span class="blue-text small-bold">DATES</span>
          </v-card-title>
          <v-card-text>
            <table>
              <tbody>
                <tr>
                  <td class="td-header">Reported Date</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.reportedDate"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Requester Affected Date</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.requesterAffectedDate"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Target Finish</td>
                  <td>
                    <input type="date"
                           v-model="targetFinish"
                           class="border-input" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Standard T.A.T</td>
                  <td>
                    <input type="text"
                           v-model="serviceRequest.standardTAT"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Actual Start</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.actualStartDate"
                           class="border-input"
                           :readonly="isViewMode" />
                  </td>
                </tr>
                <tr>
                  <td class="td-header">Actual Finish</td>
                  <td>
                    <input type="datetime-local"
                           v-model="serviceRequest.actualFinishDate"
                           class="border-input"
                           :readonly="!isCheckerMode" />
                  </td>
                </tr>
              </tbody>
            </table>
          </v-card-text>
        </v-card>

        <!-- TIME TRACKING (TIMELINE) -->
        <v-card class="mb-5 mt-2" v-if="timeTracking.length > 0">
          <v-card-title>
            <span class="blue-text small-bold">TIME TRACKING</span>
          </v-card-title>
          <v-card-text>
            <v-timeline>
              <v-timeline-item v-for="(entry, index) in timeTracking"
                               :key="index"
                               :color="entry.color"
                               fill-dot>
                <v-card elevation="2">
                  <v-card-title>{{ entry.memo }}</v-card-title>
                  <v-card-text>
                    <div>Date: {{ entry.date }}</div>
                    <div>Person: {{ entry.person }}</div>
                  </v-card-text>
                </v-card>
              </v-timeline-item>
            </v-timeline>
          </v-card-text>
        </v-card>


        <!-- ACTION BUTTONS -->
        <div class="mt-4">
          <button v-if="!isViewMode && !isMakerMode"
                  type="button"
                  @click="submitServiceRequest">
            Submit
          </button>
          <button v-if="isMakerMode"
                  type="button"
                  @click="submitMakerRequest">
            Maker Submit
          </button>
          <button v-if="isCheckerMode"
                  type="button"
                  @click="submitCheckerRequest">
            Checker Submit
          </button>
          <button v-if="isViewMode"
                  type="button"
                  @click="backToList">
            Return to List
          </button>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex'
  import { Base64 } from 'js-base64'
  import moment from 'moment';
  import Swal from 'sweetalert2'

  export default {
    name: 'ServiceRequestForm',
    data() {
      return {
        loading: false,
        isViewMode: false,
        isMakerMode: false,
        isCheckerMode: false,
        targetFinish: '',
        serviceRequest: {
          id: 0,
          serviceRequestNo: '',
          owner: '',
          ownerGroup: '',
          status: 'NEW',
          statusDate: '',
          fixStatus: '',
          fixStatusDate: '',
          paymentStatus: '',
          paymentStatusDate: '',
          requester: '',
          requesterName: '',
          requesterPhone: '',
          requesterEmail: '',
          reportedBy: '',
          reportedByName: '',
          reportedByPhone: '',
          reportedByEmail: '',
          classificationPath: '',
          services: '',   // <-- Will be filled by the 'Services' dropdown
          category: '',   // <-- Will be filled by the 'Category' dropdown
          section: '',
          timeTracking: '',
          summary: '',
          details: '',
          reportedDate: '',
          requesterAffectedDate: '',
          targetFinishDate: '',
          standardTAT: '',
          actualStartDate: '',
          actualFinishDate: '',
          workNotes: '',
          verificationNotes: ''
        },

        // Classification / Service / Category data
        classificationData: [
          {
            name: 'General Services (GS)',
            services: [
              {
                name: 'Card Access',
                section: 'General Services (GS)',
                categories: [
                  { name: 'New Request', tatDisplay: '3 working days' },
                  { name: 'Request for visitor/vendor/dept card', tatDisplay: '1 working day' },
                  { name: 'Request for event report (attendance report)', tatDisplay: '5 working days' },
                  { name: 'Change/Add Access/Timezone', tatDisplay: '5 working days' },
                  { name: 'Request for Nursing Room Access', tatDisplay: '7 working days' },
                  { name: 'Deletion', tatDisplay: '3 working days' }
                ]
              },
              {
                name: 'Telecommunication',
                section: 'General Services (GS)',
                categories: [
                  { name: 'Request for new IP phones', tatDisplay: '7 working days' },
                  { name: 'Request for new Telekom lines', tatDisplay: '5 working days' },
                  { name: 'Relocation of IP phones (CSQ)', tatDisplay: '7 working days' },
                  { name: 'Relocation of Telekom direct lines', tatDisplay: '10 working days' },
                  { name: 'Faulty phone lines (sets, voice recording)', tatDisplay: '7 working days' },
                  { name: 'Astro problems', tatDisplay: '3 working days' }
                ]
              },
              {
                name: 'Movers',
                section: 'General Services (GS)',
                categories: [
                  { name: 'Seeking quotation', tatDisplay: '5 working days' }
                ]
              },
              {
                name: 'Water Dispenser',
                section: 'General Services (GS)',
                categories: [
                  { name: 'Request for new Water Dispenser', tatDisplay: '7 working days' },
                  { name: 'Water filter problems/replacement', tatDisplay: '3 working days' }
                ]
              },
              {
                name: 'Mailroom Arrangement',
                section: 'General Services (GS)',
                categories: [
                  { name: 'Request for Despatch/Courier Service', tatDisplay: '1 working day' },
                  { name: 'Request for courier material', tatDisplay: '3 working days' }
                ]
              },
              {
                name: 'Records Management',
                section: 'General Services (GS)',
                categories: [
                  { name: 'Creation of new account', tatDisplay: '3 working days' },
                  { name: 'Escalate Destruction Request', tatDisplay: '5 working days' },
                  { name: 'Seeking quotation for ad-hoc destruction', tatDisplay: '5 working days' }
                ]
              },
              {
                name: 'Security Guarding Services',
                section: 'General Services (GS)',
                categories: [
                  { name: 'Reporting guards absence', tatDisplay: '1 working day' },
                  { name: 'Reporting guards service level', tatDisplay: '3 working days' }
                ]
              },
              {
                name: 'Photocopier',
                section: 'General Services (GS)',
                categories: [
                  { name: 'New Request (New staff)', tatDisplay: '3 working days' },
                  { name: 'Request for Color Access', tatDisplay: '5 working days' },
                  { name: 'Deletion (Exit Staff)', tatDisplay: '3 working days' }
                ]
              }
            ]
          },
          {
            name: 'Facilities Management (FM)',
            services: [
              {
                name: 'Lights Replacement',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Light Tubes & Bulbs - CSQ & 3A', tatDisplay: '3 working days' },
                  { name: 'Light Tubes & Bulbs - Branch', tatDisplay: '5 working days' }
                ]
              },
              {
                name: 'Electrical Services',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Power Trip - No Power Supply', tatDisplay: '1 working day' }
                ]
              },
              {
                name: 'Air Conditioning System',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Temperature issue', tatDisplay: '1 working day' },
                  { name: 'Air Cond leaking (faulty service)', tatDisplay: '3 working days' },
                  { name: 'Repair Air Cond @ server - computer room', tatDisplay: '7 working days' },
                  { name: 'Air Cond Replacement New', tatDisplay: '10 working days' },
                  { name: 'Extend Centralised Air Cond Hours', tatDisplay: '3 working days' }
                ]
              },
              {
                name: 'Fire Fighting System',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Fire Fighting System - Renewal license, Replace Extinguishers', tatDisplay: '10 working days' },
                  { name: 'Fire Fighting System - Fire Panel & system', tatDisplay: '7 working days' },
                  { name: 'Fire Fighting System - Emergency Lights Keluar sign', tatDisplay: '7 working days' }
                ]
              },
              {
                name: 'Genset/UPS',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Genset & UPS System - No Power, Not Function, Diesel top up', tatDisplay: '1 working day' },
                  { name: 'Genset & UPS System - Repair, Parts Replacement', tatDisplay: '5 working days' },
                  { name: 'Genset & UPS System - New Replacement', tatDisplay: '10 working days' },
                  { name: 'Genset & System - Deep rectification', tatDisplay: '14 working days' }
                ]
              },
              {
                name: 'Plumbing',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Plumbing Sanitary - Clog, leakage, drainage', tatDisplay: '2 working days' },
                  { name: 'Plumbing Sanitary - Water Supply Issue', tatDisplay: '1 working day' }
                ]
              },
              {
                name: 'Card Access System (Hardware)',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'Door cannot access - beeping door, Magnetic door faulty', tatDisplay: '1 working day' },
                  { name: 'Parts replacement', tatDisplay: '7 working days' }
                ]
              },
              {
                name: 'Security (CCTV / Alarm)',
                section: 'Facilities Management (FM)',
                categories: [
                  { name: 'CCTV - Attend to Service Request', tatDisplay: '5 working days' },
                  { name: 'CCTV - Request for Footage', tatDisplay: '7 working days' },
                  { name: 'CCTV - Request for Additional Camera', tatDisplay: '10 working days' },
                  { name: 'Alarm - Attend to Service Request', tatDisplay: '5 working days' },
                  { name: 'Alarm - Change Requests', tatDisplay: '7 working days' },
                  { name: 'Alarm - CMS down', tatDisplay: '1 working day' }
                ]
              }
            ]
          },
          {
            name: 'Administration Compliance & Finance',
            services: [
              {
                name: 'Fixed Asset Disposal',
                section: 'Administration Compliance & Finance',
                categories: [
                  { name: 'Disposal of Fixed Asset - Quotation', tatDisplay: '5 working days' },
                  { name: 'Disposal of Fixed Asset - Seeking Approval', tatDisplay: '7 working days' },
                  { name: 'Disposal of Fixed Asset - Disposal Arrangement', tatDisplay: '10 working days' }
                ]
              },
              {
                name: 'Utilities',
                section: 'Administration Compliance & Finance',
                categories: [
                  { name: 'Termination of electricity and water supply', tatDisplay: '3 working days' }
                ]
              }
            ]
          },
          {
            name: 'Project Management',
            services: [
              {
                name: 'Minor Renovation',
                section: 'Project Management',
                categories: [
                  { name: 'Off the Shelf product', tatDisplay: '10 working days' },
                  { name: 'Renovation (value above RM250k)', tatDisplay: '14 working days' }
                ]
              }
            ]
          },
          {
            name: 'Real Estate Management',
            services: [
              {
                name: 'Real Estate Services',
                section: 'Real Estate Management',
                categories: [
                  { name: 'Tenancy matters', tatDisplay: '7 working days' },
                  { name: 'Repairs work (LL)', tatDisplay: '10 working days' }
                ]
              }
            ]
          }
        ],

        timeTracking: [
        ],

        decodedQueryParams: {}
      };
    },
    computed: {
      ...mapGetters(['loggedInUser']),

      /**
       * Looks up the classification object from classificationData
       * whose "name" matches serviceRequest.classificationPath.
       * Then returns the 'services' array from that object.
       */
      availableServices() {
        const foundClassification = this.classificationData.find(
          (c) => c.name === this.serviceRequest.classificationPath
        );
        return foundClassification ? foundClassification.services : [];
      },

      /**
       * Finds the chosen service object from 'availableServices'
       * whose 'name' matches serviceRequest.services
       * Then returns the 'categories' array from that object.
       */
      availableCategories() {
        const chosenService = this.availableServices.find(
          (s) => s.name === this.serviceRequest.services
        );
        return chosenService ? chosenService.categories : [];
      }
    },
    async created() {
      this.loading = true;
      try {
        // 1) If there's a "params" param, decode it
        if (this.$route.query.params) {
          this.decodedQueryParams = JSON.parse(Base64.decode(this.$route.query.params));
          if (this.decodedQueryParams.id) {
            await this.fetchServiceRequest(this.decodedQueryParams.id);
          }
          if (this.decodedQueryParams.view === true) {
            this.isViewMode = true;
          }
          if (this.decodedQueryParams.maker === true) {
            this.isMakerMode = true;
          }
          if (this.decodedQueryParams.checker === true) {
            this.isCheckerMode = true;
          }
        }
        // 2) Otherwise, check for plain ?id=2&view=true
        else {
          const { id, view, maker, checker } = this.$route.query;
          if (id) {
            await this.fetchServiceRequest(id);
          }
          if (view === 'true') {
            this.isViewMode = true;
          }
          if (maker === 'true') {
            this.isMakerMode = true;
          }
          if (checker === 'true') {
            this.isCheckerMode = true;
          }
        }

        const now = new Date();
        this.serviceRequest.statusDate = this.toDateTimeLocal(now);  // ✅ Use `this.toDateTimeLocal()`
        this.serviceRequest.reportedDate = this.toDateTimeLocal(now);
        this.serviceRequest.requesterAffectedDate = this.toDateTimeLocal(now);
        this.serviceRequest.actualStartDate = this.toDateTimeLocal(now);

        if (this.isMakerMode) {
          this.serviceRequest.fixStatusDate = this.toDateTimeLocal(now); // ✅ No more errors
        }
        if (this.isCheckerMode) {
          this.serviceRequest.paymentStatusDate = this.toDateTimeLocal(now); // ✅ No more errors
          this.serviceRequest.actualFinishDate = this.toDateTimeLocal(now); // ✅ No more errors
        }
      } catch (error) {
        console.error('Error parsing query params:', error);
      }
      this.loading = false;
    },
    methods: {
      async fetchAuditTrail(requestId) {
        try {
          const responseAudit = await this.$axios.get(
            `${this.$config.restUrl}/api/audittrail/GetServiceRequestAuditTrail`,
            { params: { id: requestId } }
          );

          // responseAudit.data.data is presumably the array of AuditTrailDTO
          const audits = Array.isArray(responseAudit.data.data) ? responseAudit.data.data : [];

          // Simple list of colors to randomize from
          const colorOptions = ['blue', 'green', 'teal', 'purple', 'orange', 'brown', 'red', 'pink'];

          // Map each audit record to a timeline item
          this.timeTracking = audits.map(audit => ({
            memo: audit.action || `Action: ${audit.action}`,    // Or use something else
            date: audit.actionTime
              ? moment(audit.actionTime).format('DD MMM YY HH:mm')
              : moment(audit.createdOnUTC).format('DD MMM YY HH:mm'),
            person: audit.actorStr || 'System',
            color: colorOptions[Math.floor(Math.random() * colorOptions.length)]
          }));
        } catch (error) {
          console.error('Error fetching audit trail:', error);
          // If it fails, timeTracking remains empty or as-is
        }
      },
      // Helper to format a Date into "YYYY-MM-DDTHH:MM" for datetime-local.
      toDateTimeLocal(dateObj) {
        const pad = (n) => (n < 10 ? '0' + n : n);
        const yyyy = dateObj.getFullYear();
        const mm = pad(dateObj.getMonth() + 1);
        const dd = pad(dateObj.getDate());
        const hh = pad(dateObj.getHours());
        const min = pad(dateObj.getMinutes());
        return `${yyyy}-${mm}-${dd}T${hh}:${min}`;
      },
      async submitMakerRequest() {
        if (!this.serviceRequest.fixStatus) {
          Swal.fire("Warning", "Fix Status is required!", "warning");
          return;
        }

        try {
          this.loading = true;
          await this.$axios.post(`${this.$config.restUrl}/api/serviceRequest/makerRequest`, {
            data: {
              id: this.serviceRequest.id,
              fixStatus: this.serviceRequest.fixStatus,
              fixStatusDate: this.serviceRequest.fixStatusDate,
              workNotes: this.serviceRequest.workNotes,
              targetFinishDate: this.serviceRequest.targetFinishDate || new Date().toISOString()
            }
          });

          Swal.fire("Success", "Maker request submitted successfully!", "success").then(() => {
            if (this.hasAccessToRequestPage()) {
              this.$router.push({ path: '/request' });
            } else {
              this.$router.push({ path: '/dashboard' });
            }
          });
        } catch (error) {
          console.error("Error submitting maker request:", error);
          Swal.fire("Error", "Failed to submit the maker request.", "error");
        } finally {
          this.loading = false;
        }
      },
      async submitCheckerRequest() {
        if (!this.serviceRequest.fixStatus) {
          Swal.fire("Warning", "Payment Status is required!", "warning");
          return;
        }

        try {
          this.loading = true;
          await this.$axios.post(`${this.$config.restUrl}/api/serviceRequest/checkerRequest`, {
            data: {
              id: this.serviceRequest.id,
              paymentStatus: this.serviceRequest.paymentStatus,
              paymentStatusDate: this.serviceRequest.paymentStatusDate,
              verificationNotes: this.serviceRequest.verificationNotes,
              actualFinishDate: this.serviceRequest.actualFinishDate || new Date().toISOString()
            }
          });

          Swal.fire("Success", "Checker request submitted successfully!", "success").then(() => {
            if (this.hasAccessToRequestPage()) {
              this.$router.push({ path: '/request' });
            } else {
              this.$router.push({ path: '/dashboard' });
            }
          });
        } catch (error) {
          console.error("Error submitting checker request:", error);
          Swal.fire("Error", "Failed to submit the checker request.", "error");
        } finally {
          this.loading = false;
        }
      },
      // This is only used in the table, if you do a "View" button
      viewRequest(item) {
        const queryParams = {
          id: item.id,
          view: true
        };
        const encodedParams = Base64.encode(JSON.stringify(queryParams));
        this.$router.push({
          path: '/serviceQuotation',
          query: { params: encodedParams }
        });
      },

      onClassificationChange() {
        // Clear out any previously selected service/category
        this.serviceRequest.services = '';
        this.serviceRequest.category = '';
        this.serviceRequest.section = '';
        this.serviceRequest.timeTracking = '';
        this.targetFinish = '';
      },
      onServiceChange() {
        // Find the chosen service in availableServices
        const chosenService = this.availableServices.find(
          (s) => s.name === this.serviceRequest.services
        );
        // If found, set the relevant fields
        if (chosenService) {
          this.serviceRequest.section = chosenService.section;
        } else {
          this.serviceRequest.section = '';
        }
        // Clear category, TAT, etc.
        this.serviceRequest.category = '';
        this.serviceRequest.timeTracking = '';
        this.targetFinish = '';
      },
      onCategoryChange() {
        // We can look up the chosen service again
        const chosenService = this.availableServices.find(
          (s) => s.name === this.serviceRequest.services
        );
        if (!chosenService) return;

        // Now find the selected category
        const chosenCategory = chosenService.categories.find(
          (cat) => cat.name === this.serviceRequest.category
        );
        if (!chosenCategory) {
          this.serviceRequest.timeTracking = '';
          this.targetFinish = '';
          return;
        }
        // Set TAT
        this.serviceRequest.timeTracking = chosenCategory.tatDisplay;
      },

      /**
       * Fetch existing ServiceRequest by ID
       */
      async fetchServiceRequest(id) {
        try {
          // Adjust the route name if your actual endpoint differs
          const response = await this.$axios.get(
            `${this.$config.restUrl}/api/ServiceRequest/GetServiceRequestByID`,
            { params: { id } }
          );
          if (response.data && response.data.data) {
            const sr = response.data.data;
            // Merge it into the existing serviceRequest
            this.serviceRequest = {
              ...this.serviceRequest,
              ...sr
            };
          }

          await this.fetchAuditTrail(id);
        } catch (error) {
          console.error('Error fetching service request:', error);
          Swal.fire('Error', 'Unable to load the service request.', 'error');
        }
      },

      validateForm() {
        if (!this.serviceRequest.requester) {
          return 'Requester must not be empty!';
        }
        // Additional validations as needed
        return '';
      },

      async submitServiceRequest() {
        const clientErr = this.validateForm();
        if (clientErr) {
          Swal.fire('Warning', clientErr, 'warning');
          return;
        }

        try {
          this.loading = true;
          // Post EXACTLY as { data: { ... } }
          const response = await this.$axios.post(
            `${this.$config.restUrl}/api/serviceRequest/CreateServiceRequest`,
            {
              data: {
                // Must match your ServiceRequestDTO
                id: this.serviceRequest.id || 0,
                serviceRequestNo: this.serviceRequest.serviceRequestNo || '',
                owner: this.serviceRequest.owner || '',
                ownerGroup: this.serviceRequest.ownerGroup || '',
                status: this.serviceRequest.status || 'NEW',
                statusDate: this.serviceRequest.statusDate || new Date().toISOString(),
                fixStatus: this.serviceRequest.fixStatus || '',
                fixStatusDate: this.serviceRequest.fixStatusDate || null,
                paymentStatus: this.serviceRequest.paymentStatus || '',
                paymentStatusDate: this.serviceRequest.paymentStatusDate || null,
                requester: this.serviceRequest.requester || '',
                requesterName: this.serviceRequest.requesterName || '',
                requesterPhone: this.serviceRequest.requesterPhone || '',
                requesterEmail: this.serviceRequest.requesterEmail || '',
                reportedBy: this.serviceRequest.reportedBy || '',
                reportedByName: this.serviceRequest.reportedByName || '',
                reportedByPhone: this.serviceRequest.reportedByPhone || '',
                reportedByEmail: this.serviceRequest.reportedByEmail || '',
                classificationPath: this.serviceRequest.classificationPath || '',
                services: this.serviceRequest.services || '',
                category: this.serviceRequest.category || '',
                section: this.serviceRequest.section || '',
                timeTracking: this.serviceRequest.timeTracking || '',
                summary: this.serviceRequest.summary || '',
                details: this.serviceRequest.details || '',
                reportedDate: this.serviceRequest.reportedDate || new Date().toISOString(),
                requesterAffectedDate: this.serviceRequest.requesterAffectedDate || new Date().toISOString(),
                targetFinishDate: this.serviceRequest.targetFinishDate || null,
                standardTAT: this.serviceRequest.standardTAT || '',
                actualStartDate: this.serviceRequest.actualStartDate || null,
                actualFinishDate: this.serviceRequest.actualFinishDate || null,
                workNotes: this.serviceRequest.workNotes || '',
                verificationNotes: this.serviceRequest.verificationNotes || '',

                createdOnUTC: new Date().toISOString(),
                updatedOnUTC: new Date().toISOString(),
                createdByID: this.loggedInUser.id,
                createdByStr: this.loggedInUser.userName || '',
                updatedByID: this.loggedInUser.id,
                updatedByStr: this.loggedInUser.userName || '',
                approvedBy: '',
                approvedByName: '',
                checkedBy: '',
                checkedByName: '',
                serviceRequestStatus: 10
              }
            }
          );

          if (response && response.status === 200) {
            Swal.fire('Success', 'Service request created successfully!', 'success')
              .then(() => {
                this.$router.push({ path: '/request' });
              });
          } else {
            Swal.fire('Error', 'Failed to create the service request.', 'error');
          }
        } catch (error) {
          console.error('Error creating service request:', error);
          let msg = 'Failed to create the service request.';
          if (error.response && error.response.data) {
            msg = error.response.data.message || msg;
          }
          Swal.fire('Error', msg, 'error');
        } finally {
          this.loading = false;
        }
      },
      hasAccessToRequestPage() {
        const allowedRoles = ['Manager', 'Admin', 'Requester']; // Change these to match your actual roles
        return this.loggedInUser.roles && this.loggedInUser.roles.some(role => allowedRoles.includes(role));
      },
      backToList() {
        this.$router.push({ path: '/request' });
      }
    }
  };
</script>

<style scoped>
  .create-service-request-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
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

  table {
    width: 100%;
    margin-top: 1rem;
    border-collapse: collapse;
  }

  th,
  td {
    border: 1px solid #ddd;
    padding: 8px;
    text-align: left;
  }

  .td-header {
    background-color: #003399;
    color: white;
    width: 20%;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 0.5rem;
    width: 100%;
  }

  button {
    padding: 0.5rem 1rem;
    background-color: #003399;
    color: #fff;
    border: none;
    cursor: pointer;
    margin-right: 1rem;
  }

  .blue-text {
    color: #003399;
  }

  .small-bold {
    font-weight: bold;
  }

  .card-round {
    border-radius: 20px !important;
  }
</style>
