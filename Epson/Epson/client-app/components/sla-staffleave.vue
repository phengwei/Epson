<template>
  <div class="sla-staff-leaves-container">
    <div class="sla-staff-leaves-card">
      <div class="slastaff-header">
        <h4>STAFF LEAVE MANAGEMENT</h4>
      </div>
      <v-card-text>
        <form class="form-container" @submit.prevent="validateAndSaveSLAStaffLeave">
          <div class="form-group">
            <label for="staff">Staff:</label>
            <select id="staff" v-model="selectedStaff" required class="border-input">
              <option v-for="staff in staffMembers" :value="staff.id" :key="staff.id">{{ staff.userName }}</option>
            </select>
          </div>
          <div class="form-group">
            <label for="startDate">Start Date:</label>
            <input type="date" id="startDate" v-model="startDate" :min="minLeaveDate" required class="border-input">
          </div>
          <div class="form-group">
            <label for="endDate">End Date:</label>
            <input type="date" id="endDate" v-model="endDate" :min="startDate" :max="maxEndDate" required class="border-input" :disabled="!startDate">
          </div>
          <div class="form-group">
            <label for="reason">Description:</label>
            <textarea id="reason" v-model="reason" required class="border-input"></textarea>
          </div>
          <div class="form-actions">
            <button type="submit" class="save-button">Save</button>
            <button type="button" @click="resetForm" class="cancel-button">Cancel</button>
          </div>
        </form>
      </v-card-text>
    </div>
  </div>
</template>

<script>
  import Swal from 'sweetalert2';

  export default {
    name: 'SLA-StaffLeaves',
    data() {
      return {
        startDate: null,
        endDate: null,
        reason: '',
        staffMembers: [],
        selectedStaff: '',
        staffLeaves: [],
        calendarDate: new Date(),
        disabledDates: {
          to: new Date("2023-04-29T16:00:00.000Z")
        },
        highlightedDates: {}
      };
    },
    computed: {
      minLeaveDate() {
        const today = new Date();
        return today.toISOString().split('T')[0];
      },
      maxEndDate() {
        if (this.startDate) {
          const maxDate = new Date(this.startDate);
          maxDate.setDate(maxDate.getDate() + 14);
          return maxDate.toISOString().split('T')[0];
        }
        return null;
      }
    },
    watch: {
      startDate(newValue) {
        if (!newValue) {
          this.endDate = null;
        }
      },
      staffLeaves: {
        handler(newValue) {
          if (newValue) {
            this.highlightedDates = {
              customPredictor(date) {
                return newValue.some(leave =>
                  new Date(leave.startDate).toDateString() <= date.toDateString() &&
                  new Date(leave.endDate).toDateString() >= date.toDateString()
                );
              }
            };
          }
        },
        deep: true
      },
      selectedStaff: {
        handler(newValue) {
          if (newValue) {
            this.getSLAStaffLeavesByStaff(newValue);
          }
        },
        immediate: true
      }
    },
    mounted() {
      this.getAllStaffs();
    },
    methods: {
      validateAndSaveSLAStaffLeave() {
        if (this.endDate && this.endDate < this.startDate) {
          Swal.fire({
            icon: 'error',
            title: 'Invalid Date Range',
            text: 'The end date cannot be earlier than the start date.',
          });
        } else {
          this.saveSLAStaffLeave();
        }
      },
      resetForm() {
        this.startDate = null;
        this.endDate = null;
        this.reason = '';
        this.selectedStaff = '';
      },
      async getAllStaffs() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/customer/getallstaff`);
          this.staffMembers = response.data.data || [];

        } catch (error) {
          console.error('There was a problem fetching the staff:', error);
        }
      },
      async saveSLAStaffLeave() {
        try {
          const payload = {
            Data: {
              StaffId: this.selectedStaff,
              StartDate: this.startDate,
              EndDate: this.endDate,
              Reason: this.reason
            }
          };
          await this.$axios.post(`${this.$config.restUrl}/api/sla/addslastaffleave`, payload);
          Swal.fire({
            title: 'Success!',
            text: 'SLA staff leave added successfully.',
            icon: 'success',
            confirmButtonText: 'OK'
          }).then((result) => {
            if (result.isConfirmed) {
              location.reload();
            }
          });
        } catch (error) {
          console.error('There was a problem adding SLA staff leave:', error);
        }
      },
      async getSLAStaffLeavesByStaff(staffId) {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/sla/getslastaffleavesbystaff`, {
            params: {
              staffId
            }
          });
          this.staffLeaves = response.data.data || [];

        } catch (error) {
          console.error('There was a problem fetching SLA staff leaves:', error);
        }
      }
    }
  };
</script>

<style scoped>
  .sla-staff-leaves-container {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
    height: 100%;
  }

  .sla-staff-leaves-card {
    background-color: #fff;
    border-radius: 10px;
    padding: 20px;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
    width: 100%;
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }

  .slastaff-header {
    width: 100%;
    text-align: left;
    margin-bottom: 1rem;
  }

    .slastaff-header h4 {
      color: #003399;
      font-size: 1.2rem;
      font-weight: bold;
      margin: 0;
    }

  .form-container {
    width: 100%;
  }

  .form-group {
    margin-bottom: 1rem;
  }

  label {
    font-weight: bold;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 5px;
    padding: 0.5rem;
    width: 100%;
  }

  textarea {
    width: 100%;
    height: 100px;
    padding: 0.5rem;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .form-actions {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 1rem;
  }

  button {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 5px;
    cursor: pointer;
  }

  .save-button {
    background-color: #003399;
    color: white;
  }

  .cancel-button {
    background-color: #ccc;
  }
</style>
