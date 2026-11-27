using Mitsu.Core.Domain;
using Mitsu.Core.Production;

namespace Mitsu.Core.Tests;

public class MachineNumberTests
{
    [Fact]
    public void Ascending_keeps_leading_zeros()
    {
        var s = new MachineNumberSequence("B-", "0009", SerialDirection.Ascending);
        Assert.Equal("B-0009", s.MachineNoAt(0));
        Assert.Equal("B-0010", s.MachineNoAt(1));
        Assert.True(s.Validate(5).IsValid);
    }

    [Fact]
    public void Descending_counts_down_and_rejects_going_below_zero()
    {
        var s = new MachineNumberSequence("T-", "0003", SerialDirection.Descending);
        Assert.Equal("T-0002", s.MachineNoAt(1));
        Assert.Equal("T-0000", s.MachineNoAt(3));
        Assert.True(s.Validate(4).IsValid);
        Assert.False(s.Validate(5).IsValid);
    }

    [Fact]
    public void Ascending_rejects_width_overflow()
    {
        var s = new MachineNumberSequence("X", "998", SerialDirection.Ascending);
        Assert.True(s.Validate(2).IsValid);   // 998, 999
        Assert.False(s.Validate(3).IsValid);  // 1000 would change length
    }

    [Fact]
    public void Small_serial_with_many_boxes_is_allowed_in_ascending_mode()
    {
        // The 2.1.2.6 rule "總箱數 ≥ 流水號 → refuse" blocked this; 20160530 removed it.
        var s = new MachineNumberSequence("21", "0001", SerialDirection.Ascending);
        Assert.True(s.Validate(500).IsValid);
    }

    [Theory]
    [InlineData("", "0001")]
    [InlineData("21", "")]
    [InlineData("21", "12A4")]
    public void Invalid_input(string prefix, string serial)
    {
        Assert.False(new MachineNumberSequence(prefix, serial, SerialDirection.Ascending).Validate(1).IsValid);
    }

    [Fact]
    public void Advance_for_resume()
    {
        var s = new MachineNumberSequence("21", "0098", SerialDirection.Ascending).Advance(3);
        Assert.Equal("0101", s.StartSerial);
    }
}

public class PayloadTests
{
    [Fact]
    public void Composes_legacy_18_fields_in_ADT_order()
    {
        var c = new CartonPayloadComposer();
        var st = c.ComposeStatic(TestData.Product, "6785691");
        var v = c.Compose(st, TestData.Product, "21678569136");

        Assert.Equal(20, v.Length);
        Assert.Equal("MODEL", v[(int)PayloadField.ModelLabel]);
        Assert.Equal("HSN2.4S-51", v[(int)PayloadField.ModelValue]);
        Assert.Equal("50mm", v[(int)PayloadField.BoreValue]);
        Assert.Equal("0.4kW", v[(int)PayloadField.OutputValue]);
        Assert.Equal("1", v[(int)PayloadField.PhaseValue]);
        Assert.Equal("φ", v[(int)PayloadField.PhaseUnit]);
        Assert.Equal("100V     50Hz", v[(int)PayloadField.VoltageHz]);
        Assert.Equal("6785691", v[(int)PayloadField.MadeNo]);
        Assert.Equal("A01HN-2DS51   21678569136", v[(int)PayloadField.CodeAndMachineNo]);
        Assert.Equal("CODE:", v[(int)PayloadField.CodeLabel]);
        Assert.Equal("No:", v[(int)PayloadField.MachineNoLabel]);
        Assert.Equal("21678569136", v[(int)PayloadField.MachineNo]);
        Assert.Equal("HSN2.4S-51", v[(int)PayloadField.ModelValueSide]);
        Assert.Equal("MADE BY TSURUMI TAIWAN", v[(int)PayloadField.Place]);
        Assert.Equal("", v[18]);
        Assert.Equal("", v[19]);
    }

    [Fact]
    public void Empty_values_do_not_print_lonely_units()
    {
        var p = TestData.Product with { BoreValue = "", HzValue = "", PhaseValue = "" };
        var c = new CartonPayloadComposer();
        var v = c.ComposeStatic(p, "1");
        Assert.Equal("", v[(int)PayloadField.BoreValue]);
        Assert.Equal("", v[(int)PayloadField.PhaseUnit]);
        Assert.Equal("100V", v[(int)PayloadField.VoltageHz]);
    }

    [Fact]
    public void Static_values_are_not_mutated_per_carton()
    {
        var c = new CartonPayloadComposer();
        var st = c.ComposeStatic(TestData.Product, "1");
        c.Compose(st, TestData.Product, "A1");
        Assert.Equal("", st[(int)PayloadField.MachineNo]);
    }
}

public class JobValidationTests
{
    [Fact]
    public void Sample_job_is_valid()
    {
        var r = TestData.Job().Validate();
        Assert.True(r.IsValid, r.ToString());
    }

    [Fact]
    public void Box_counts()
    {
        Assert.False(TestData.Job(boxesDone: 5, total: 5).Validate().IsValid);
        Assert.False(TestData.Job(total: 0).Validate().IsValid);
        Assert.Equal(3, TestData.Job(boxesDone: 2, total: 5).RemainingBoxes);
    }

    [Fact]
    public void Field_too_long()
    {
        var job = TestData.Job() with { Product = TestData.Product with { ModelValue = new string('X', 22) } };
        var r = job.Validate();
        Assert.Contains(r.Errors, e => e.Field == "MODEL_value");
    }

    [Fact]
    public void Carton_end_beyond_L_plus_W_and_mismatched_case()
    {
        var job = TestData.Job() with { Carton = TestData.Carton with { OutsideEnd = 600 } };
        Assert.Contains(job.Validate().Errors, e => e.Field == "表面印字終了");

        job = TestData.Job() with { Carton = TestData.Carton with { CaseNo = "01153" } };
        Assert.Contains(job.Validate().Errors, e => e.Field == "紙箱編號");
    }

    [Fact]
    public void Descending_not_enough_serials()
    {
        var job = TestData.Job(total: 10, serial: "0005", dir: SerialDirection.Descending);
        Assert.Contains(job.Validate().Errors, e => e.Field == "機番流水號");
    }
}

public class ProductionJobTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Numbers_box_and_machine_no_from_resume_point()
    {
        var job = new ProductionJob(TestData.Job(boxesDone: 10, total: 13, serial: "0100"), 1);
        var a = job.CreateNext(T0)!;
        var b = job.CreateNext(T0)!;
        var c = job.CreateNext(T0)!;
        Assert.Null(job.CreateNext(T0));
        Assert.Equal((11, "210100"), (a.BoxNo, a.MachineNo));
        Assert.Equal((12, "210101"), (b.BoxNo, b.MachineNo));
        Assert.Equal(13, c.BoxNo);
        Assert.Equal("21", job.Spec.MachineNoPrefix);
        Assert.Equal("210101", b.Values[(int)PayloadField.MachineNo]);
    }

    [Fact]
    public void Carton_is_printed_only_when_all_targets_confirm()
    {
        var job = new ProductionJob(TestData.Job(total: 3), 2);
        job.CreateNext(T0); job.CreateNext(T0);

        Assert.Empty(job.Confirm(0, 2, T0));
        Assert.Equal(0, job.PrintedCount);
        Assert.Equal(2, job.Skew);

        var done = job.Confirm(1, 1, T0);
        Assert.Single(done);
        Assert.Equal(CartonState.Printed, job.Cartons[0].State);
        Assert.Equal(CartonState.Sent, job.Cartons[1].State);
    }

    [Fact]
    public void Confirm_rejects_impossible_counts()
    {
        var job = new ProductionJob(TestData.Job(total: 3), 1);
        job.CreateNext(T0);
        Assert.Throws<InvalidOperationException>(() => job.Confirm(0, 2, T0));
        job.Confirm(0, 1, T0);
        Assert.Throws<InvalidOperationException>(() => job.Confirm(0, 0, T0));
    }

    [Fact]
    public void Close_in_flight_voids_or_marks_uncertain()
    {
        var job = new ProductionJob(TestData.Job(total: 5, serial: "0001"), 2);
        for (int i = 0; i < 3; i++) job.CreateNext(T0);
        job.Confirm(0, 2, T0);
        job.Confirm(1, 1, T0);

        job.CloseInFlight(buffersCleared: true);
        Assert.Equal(CartonState.Printed, job.Cartons[0].State);
        Assert.Equal(CartonState.Uncertain, job.Cartons[1].State); // printed on one side only
        Assert.Equal(CartonState.Voided, job.Cartons[2].State);
        Assert.True(job.HasUncertain);
        Assert.Equal((1, "0002"), job.ResumePoint);
    }

    [Fact]
    public void Without_confirmed_clear_everything_in_flight_is_uncertain()
    {
        var job = new ProductionJob(TestData.Job(total: 5), 1);
        job.CreateNext(T0); job.CreateNext(T0);
        job.CloseInFlight(buffersCleared: false);
        Assert.All(job.Cartons, c => Assert.Equal(CartonState.Uncertain, c.State));
    }

    [Fact]
    public void Invalid_spec_is_rejected_before_anything_happens()
    {
        Assert.Throws<DomainValidationException>(() => new ProductionJob(TestData.Job(boxesDone: 5, total: 5), 1));
    }
}
