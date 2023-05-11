<template>
  <div class="sla-holiday-management">
    <h1>SLA - Holiday Calendar Management</h1>
    <form class="form-container">
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
      <button type="submit" @click="saveSLAHoliday">Add Holiday</button>
    </form>
  </div>
</template>


<script>
  import { mapGetters } from 'vuex';
  export default {
    name: 'SLA-Holiday',
    middleware: 'auth',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        holidayDate: null,
        description: '',
        isAdhoc: false
      };
    },
    methods: {
      async saveSLAHoliday() {
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/sla/addslaholiday`, {
            data: {
              Date: this.holidayDate,
              Description: this.description,
              IsAdhoc: this.isAdhoc
            }
          }).then(response => {
            console.log('SLA holiday added successfully');
          })
        } catch (error) {
          console.error('There was a problem adding SLA holiday');
        }
      },
      async getSLAHolidays() {
        try {
          this.loading = true
          await this.$axios.get(`${this.$config.restUrl}/api/sla/getslaholidays`).then(result => {
            this.holidayDate = result.data.data.date
            this.description = result.data.data.description
            this.isAdhoc = result.data.data.isAdhoc
          })

        } catch (error) {
          console.error('There was a problem fetching the SLA holidays:', error);
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
