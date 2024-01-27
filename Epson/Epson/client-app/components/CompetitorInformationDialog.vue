<template>
  <v-dialog v-model="localDialogCompetitor" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">Competitor Information</span>
      </v-card-title>
      <v-card-text>
        <div class="form-group">
          <label>Model</label>
          <input type="text" v-model="localCompetitor.model" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Brand</label>
          <input type="text" v-model="localCompetitor.brand" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Disty Price</label>
          <input v-model="localCompetitor.distyPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Dealer Price</label>
          <input v-model="localCompetitor.dealerPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>End User Price</label>
          <input v-model="localCompetitor.endUserPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="onAddCompetitor">Add</v-btn>
        <v-btn color="secondary" @click="onCancel">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
  export default {
    props: {
      dialogCompetitor: Boolean,
      competitor: Object,
      isViewMode: Boolean
    },
    data() {
      return {
        localDialogCompetitor: this.dialogCompetitor,
        localCompetitor: { ...this.competitor }
      };
    },
    watch: {
      dialogCompetitor(newVal) {
        this.localDialogCompetitor = newVal;
      },
      localDialogCompetitor(newVal) {
        this.$emit('update:dialogCompetitor', newVal);
      },
      competitor: {
        handler(newVal) {
          this.localCompetitor = { ...newVal };
        },
        deep: true,
      }
    },
    methods: {
      onAddCompetitor() {
        if (this.localCompetitor.brand && this.localCompetitor.model && this.localCompetitor.endUserPrice) {
          this.$emit('add-competitor', this.localCompetitor);
          this.localCompetitor = {
            model: null,
            brand: null,
            distyPrice: null,
            dealerPrice: null,
            endUserPrice: null,
          };
          this.localDialogCompetitor = false;

        } else {
          this.$swal('Error', 'Please fill out all competitor fields', 'error');
        }
      },
      onCancel() {
        this.localDialogCompetitor = false;
      }
    }
  };
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


