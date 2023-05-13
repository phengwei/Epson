<template>
  <div class="sla-holiday-management">
    <h1>SLA - Holiday Calendar Management</h1>
    <form class="form-container">
      <div class="form-group">
        <label for="calendar">Holiday Calendar:</label>
        <holidaydatepicker :existing-holidays="existingHolidays" :is-editable="false"></holidaydatepicker>
      </div>
      <div class="form-group">
        <label for="holidayDate">Holiday Date:</label>
        <input type="date" id="holidayDate" v-model="holidayDate" required class="border-input">
      </div>
      <div class="form-group">
        <label for="description">Description:</label>
        <input type="text" id="description" v-model="description" required class="border-input">
      </div>
      <div class="form-group">
        <label for="isAdhoc">Is Adhoc:</label>
        <input type="checkbox" id="isAdhoc" v-model="isAdhoc">
      </div>
      <button type="submit" @click.prevent="saveSLAHoliday">Add Holiday</button>
    </form>
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
      async saveSLAHoliday() {
        try {
          const holidayDateString = new Date(this.holidayDate).toISOString();
          const holidayDate = new Date(holidayDateString);
          holidayDate.setHours(holidayDate.getHours() - 8);
          const updatedHolidayDateString = holidayDate.toISOString();

          if (this.existingHolidays.includes(updatedHolidayDateString)) {
            Swal.fire({
              title: 'Error!',
              text: 'This date is already a holiday.',
              icon: 'error',
              confirmButtonText: 'OK'
            });
            return;
          }

          if (!this.holidayDate) {
            Swal.fire({
              title: 'Error!',
              text: 'Please select a holiday date.',
              icon: 'error',
              confirmButtonText: 'OK'
            });
            return;
          }

          await this.$axios.post(`${this.$config.restUrl}/api/sla/addslaholiday`, {
            data: {
              Date: this.holidayDate,
              Description: this.description,
              IsAdhoc: this.isAdhoc
            }
          });
          console.log('SLA holiday added successfully');
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


<style scoped>
  .sla-holiday-management {
    display: flex;
    justify-content: flex-end;
    align-items: flex-start;
    flex-direction: column;
    margin: 10rem;
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

    input[type="checkbox"]:checked {
      background-color: #4285f4;
      border-color: #4285f4;
    }

  .form-group {
    margin-bottom: 1rem;
  }

  label {
    display: block;
    font-weight: bold;
    margin-bottom: 0.5rem;
  }

  button {
    padding: 0.5rem 1rem;
    background-color: #003399;
    color: #fff;
    border: none;
    cursor: pointer;
  }

</style>
