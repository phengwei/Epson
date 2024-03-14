<template>
  <div class="sla-staff-leaves-management">
    <h1>SLA - Staff Leaves Management</h1>
    <form class="form-container">
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
        <label for="reason">Reason:</label>
        <textarea id="reason" v-model="reason" required></textarea>
      </div>
      <button type="submit" @click="validateAndSaveSLAStaffLeave">Add Staff Leave</button>
    </form>
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
      async getAllStaffs() {
        try {
          const response = await fetch('api/customer/getallstaff');
          const { data } = await response.json();
          this.staffMembers = data;

          const t = [
            "2023-04-29T16:00:00.000Z",
            "2023-05-04T16:00:00.000Z",
            "2023-05-03T16:00:00.000Z",
            "2023-05-09T16:00:00.000Z"
          ];

        } catch (error) {
          console.error('There was a problem fetching the staff:', error);
        }
      },
      async saveSLAStaffLeave() {
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/sla/addslastaffleave`, {
            data: {
              startDate: this.startDate,
              endDate: this.endDate,
              reason: this.reason,
              staffId: this.selectedStaff
            }
          });
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
          const { data } = await this.$axios.get(`${this.$config.restUrl}/api/sla/getslastaffleavesbystaff`, {
            params: {
              staffId
            }
          });
          const staffLeaves = data;

        } catch (error) {
          console.error('There was a problem fetching SLA staff leaves:', error);
        }
      }
    }
  };
</script>

<style scoped>
  .sla-staff-leaves-management {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    flex-direction: column;
  }

  .form-container {
    max-width: 400px;
    padding: 2rem;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 0.5rem;
    width: 100%;
  }

  .form-group {
    margin-bottom: 1rem;
  }

  label {
    display: block;
    font-weight: bold;
    margin-bottom: 0.5rem;
  }

  textarea {
    width: 100%;
    height: 100px;
    padding: 0.5rem;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  button {
    padding: 0.5rem 1rem;
    background-color: #003399;
    color: #fff;
    border: none;
    cursor: pointer;
  }

  #leaveCalendar {
    margin-top: 1rem;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .vdp-datepicker__calendar .cell.highlighted {
    background-color: #ffc107;
  }
</style>

