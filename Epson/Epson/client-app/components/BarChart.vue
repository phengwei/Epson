<script>
  import { Bar, mixins } from 'vue-chartjs'

  export default {
    extends: Bar,
    mixins: [mixins.reactiveProp],
    props: ['chartData', 'options'],
    mounted() {
      this.renderChart(this.chartData, this.options);
    },
    watch: {
      chartData() {
        this.$data._chart.update(); 
      }
    }
  }
</script>
