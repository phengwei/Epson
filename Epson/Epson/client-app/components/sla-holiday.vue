<template>
  <div class="sla-holiday-container">
    <div class="sla-holiday-card">
      <div class="slacard-header">
        <h4>HOLIDAY CALENDAR</h4>
      </div>
      <div class="holiday-calendar">
        <holidaydatepicker :existing-holidays="existingHolidays"
                           :is-editable="false"
                           class="full-width-datepicker"
                           @input="updateHolidayDate"></holidaydatepicker>

      </div>
      <form class="form-container">
        <div class="form-group">
          <label for="description">Description</label>
          <input type="text" id="description" v-model="description" required class="border-input">
        </div>
        <div class="form-actions">
          <div class="checkbox-group">
            <input type="checkbox" id="isAdhoc" v-model="isAdhoc">
            <label for="isAdhoc">Adhoc</label>
          </div>
          <button type="button" @click.prevent="saveSLAHoliday" class="save-button">Save</button>
          <button type="button" @click.prevent="resetForm" class="cancel-button">Cancel</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import Swal from 'sweetalert2';
  export default {
    name: 'SLA-Holiday',
    middleware: 'auth',
    components: {
      holidaydatepicker: () => process.client ? import('~/components/HolidayDatePicker.vue') : null
    },
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        holidayDate: null,
        description: '',
        isAdhoc: false,
        existingHolidays: []
      };
    },
    methods: {
      updateHolidayDate(date) {
        const adjustedDate = new Date(date);
        adjustedDate.setDate(adjustedDate.getDate() + 1);
        adjustedDate.setHours(0, 0, 0, 0);
        this.holidayDate = adjustedDate;
      },
      resetForm() {
        this.description = '';
        this.isAdhoc = false;
        this.holidayDate = null; 
      },
      async saveSLAHoliday() {
        if (!this.holidayDate) {
          Swal.fire({
            title: 'Error!',
            text: 'Please select a holiday date.',
            icon: 'error',
            confirmButtonText: 'OK'
          });
          return;
        }
        const formattedDate = this.holidayDate.toISOString().split('T')[0];

        try {
          await this.$axios.post(`${this.$config.restUrl}/api/sla/addslaholiday`, {
            data: {
              Date: formattedDate, 
              Description: this.description,
              IsAdhoc: this.isAdhoc
            }
          });
          Swal.fire({
            title: 'Success!',
            text: 'SLA holiday added successfully.',
            icon: 'success',
            confirmButtonText: 'OK'
          }).then((result) => {
            if (result.isConfirmed) {
              location.reload();
            }
          });
        } catch (error) {
          console.error('There was a problem adding SLA holiday');
        }
      },
      async getSLAHolidays() {
        try {
          this.loading = true;
          const response = await this.$axios.get(`${this.$config.restUrl}/api/sla/getslaholidays`);
          const responseData = response.data.data;
          if (Array.isArray(responseData)) {
            this.existingHolidays = responseData.map(item => {
              const date = new Date(item.date);
              return date.toISOString();
            });

          } else {
            console.error('SLA holidays data is missing or invalid:', responseData);
          }
        } catch (error) {
          console.error('There was a problem fetching the SLA holidays:', error);
        } finally {
          this.loading = false;
        }
      }
    },
    mounted() {
      this.getSLAHolidays();
    }
  };
</script>

<style>
  .sla-holiday-container {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
    height: 100%;
  }

  .sla-holiday-card {
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

  .slacard-header {
    width: 100%;
    text-align: left;
    margin-bottom: 1rem;
  }

    .slacard-header h4 {
      color: #003399;
      font-size: 1.2rem;
      font-weight: bold;
      margin: 0;
    }

  .holiday-calendar {
    margin-bottom: 20px;
  }

  .full-width-datepicker .vdp-datepicker__calendar {
    width: 100% !important;
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

  input[type="checkbox"] {
    margin-right: 0.5rem;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;
    border-radius: 3px;
    border: 2px solid #ccc;
    width: 1.2em;
    height: 1.2em;
  }

    input[type="checkbox"]:checked {
      background-color: #4285f4;
      border-color: #4285f4;
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
